using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Json;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    // TODO: Die DefaultToolApi wurde bewusst von Codex von den Zeilen her minimal gehalten.
    //       Bitte die einzelnen Tool Calls in eigenen Klassen abbilden und hier nur die Registrierung und Delegation vornehmen.
    public class DefaultToolApi : IFlowBloxAIToolApi
    {
        private readonly Dictionary<string, Func<JObject, CancellationToken, Task<ToolResponse>>> _handlers;
        private readonly List<ToolDefinition> _definitions;

        public DefaultToolApi()
        {
            _handlers = new(StringComparer.OrdinalIgnoreCase)
            {
                ["GetProjectJson"] = (a, c) => Task.FromResult(GetProjectJson()),
                ["GetRootCategories"] = HandleGetRootCategoriesAsync,
                ["GetCategoryChildren"] = HandleGetCategoryChildrenAsync,
                ["CreateFlowBlock"] = HandleCreateFlowBlockAsync,
                ["UpdateFlowBlock"] = HandleUpdateFlowBlockAsync,
                ["DeleteFlowBlock"] = HandleDeleteFlowBlockAsync,
                ["ConnectFlowBlocks"] = HandleConnectFlowBlocksAsync,
                ["DisconnectFlowBlocks"] = HandleDisconnectFlowBlocksAsync,
                ["CreateManagedObject"] = HandleCreateManagedObjectAsync,
                ["UpdateManagedObject"] = HandleUpdateManagedObjectAsync,
                ["DeleteManagedObject"] = HandleDeleteManagedObjectAsync,
                ["GetManagedObjectsTypes"] = HandleGetManagedObjectsTypesAsync,
                ["GetManagedObjectKindsInfo"] = HandleGetManagedObjectKindsInfoAsync,
                ["GetFlowBlockKindsInfo"] = HandleGetFlowBlockKindsInfoAsync,
                ["GetFlowBlocKindsInfo"] = HandleGetFlowBlockKindsInfoAsync,
                ["GetFlowBlockSnapshot"] = HandleGetFlowBlockSnapshotAsync,
                ["GetManagedObjectSnapshot"] = HandleGetManagedObjectSnapshotAsync,
                ["GetFlowBloxComponentSnapshotJSON"] = HandleGetFlowBloxComponentSnapshotAsync,
                ["BatchExecuteToolRequests"] = HandleBatchExecuteAsync
            };

            _definitions =
            [
                Def("GetProjectJson", "Returns active project JSON export."),
                Def("GetRootCategories", "Returns root FlowBlock categories."),
                Def("GetCategoryChildren", "Returns child categories and kinds for categoryPath.", new JObject { ["categoryPath"] = "string[]" }),
                Def("CreateFlowBlock", "Creates flow block.", new JObject { ["typeFullName"] = "string", ["name"] = "string?", ["x"] = "int?", ["y"] = "int?" }),
                Def("UpdateFlowBlock", "Updates one or many paths using JSON-path syntax /Property/0/Nested.", new JObject { ["name"] = "string", ["path"] = "string?", ["value"] = "any?", ["updates"] = "[{path,value}]?" }),
                Def("DeleteFlowBlock", "Deletes flow block by name.", new JObject { ["name"] = "string" }),
                Def("ConnectFlowBlocks", "Connects from -> to.", new JObject { ["from"] = "string", ["to"] = "string" }),
                Def("DisconnectFlowBlocks", "Disconnects from -> to.", new JObject { ["from"] = "string", ["to"] = "string" }),
                Def("CreateManagedObject", "Creates managed object.", new JObject { ["typeFullName"] = "string", ["name"] = "string?" }),
                Def("UpdateManagedObject", "Updates one or many paths using JSON-path syntax /Property/0/Nested.", new JObject { ["name"] = "string", ["path"] = "string?", ["value"] = "any?", ["updates"] = "[{path,value}]?" }),
                Def("DeleteManagedObject", "Deletes managed object.", new JObject { ["name"] = "string" }),
                Def("GetManagedObjectsTypes", "Returns managed object kinds."),
                Def("GetManagedObjectKindsInfo", "Returns managed object kind metadata.", new JObject { ["typeFullName"] = "string" }),
                Def("GetFlowBlockKindsInfo", "Returns flow block kind metadata.", new JObject { ["typeFullName"] = "string" }),
                Def("GetFlowBlockSnapshot", "Returns flow block snapshot with refs normalized to names.", new JObject { ["name"] = "string" }),
                Def("GetManagedObjectSnapshot", "Returns managed object snapshot with refs normalized to names.", new JObject { ["name"] = "string" }),
                Def("GetFlowBloxComponentSnapshotJSON", "Generic snapshot. kind=flowBlock|managedObject.", new JObject { ["kind"] = "string", ["name"] = "string" }),
                Def("BatchExecuteToolRequests", "Executes multiple tool requests.", new JObject { ["continueOnError"] = "bool?", ["requests"] = "[{toolName,arguments}]" })
            ];
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions() => _definitions;

        public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ToolName)) return Task.FromResult(Fail("ToolName is required."));
            return _handlers.TryGetValue(request.ToolName, out var h)
                ? h(request.Arguments ?? new JObject(), ct)
                : Task.FromResult(Fail($"Tool '{request.ToolName}' is not registered."));
        }

        private static ToolDefinition Def(string n, string d, JObject? s = null) => new() { Name = n, Description = d, ArgumentsSchema = s ?? new JObject() };
        private static ToolResponse Ok(JObject r) => new() { Ok = true, Result = r };
        private static ToolResponse Fail(string e, JObject? r = null) => new() { Ok = false, Error = e, Result = r ?? new JObject() };
        private static FlowBloxRegistry Reg() => FlowBloxRegistryProvider.GetRegistry() ?? throw new InvalidOperationException("FlowBlox registry is not available.");
        private static FlowBloxProject Project() => FlowBloxProjectManager.Instance.ActiveProject ?? throw new InvalidOperationException("No active project is loaded.");

        private static ToolResponse GetProjectJson()
        {
            var p = Project();
            return Ok(new JObject
            {
                ["projectJson"] = JsonConvert.SerializeObject(p, JsonSettings.ProjectExport()),
                ["flowBlockCount"] = p.FlowBloxRegistry.GetFlowBlocks().Count(),
                ["managedObjectCount"] = p.FlowBloxRegistry.GetManagedObjects().Count()
            });
        }

        private Task<ToolResponse> HandleGetRootCategoriesAsync(JObject args, CancellationToken ct)
            => Task.FromResult(Ok(new JObject
            {
                ["categories"] = new JArray(FlowBlockCategory.GetAll().Where(x => x.ParentCategory == null).OrderBy(x => x.DisplayName).Select(x => new JObject { ["displayName"] = x.DisplayName, ["path"] = new JArray(x.DisplayName) }))
            }));

        private Task<ToolResponse> HandleGetCategoryChildrenAsync(JObject args, CancellationToken ct)
        {
            var categoryPath = args["categoryPath"]?.ToObject<string[]>() ?? Array.Empty<string>();
            var category = FlowBlockCategory.GetAll().FirstOrDefault(x => PathOf(x).SequenceEqual(categoryPath));
            if (category == null) return Task.FromResult(Fail("Category path could not be resolved."));

            var childCategories = FlowBlockCategory.GetAll().Where(x => x.ParentCategory == category).OrderBy(x => x.DisplayName)
                .Select(x => new JObject { ["displayName"] = x.DisplayName, ["path"] = new JArray(PathOf(x)) });
            var kinds = Project().CreateInstances<BaseFlowBlock>().Where(x => x.GetCategory().Equals(category)).OrderBy(x => FlowBloxComponentHelper.GetDisplayName(x)).Select(ToTypeInfo);
            return Task.FromResult(Ok(new JObject { ["category"] = category.DisplayName, ["childCategories"] = new JArray(childCategories), ["flowBlockKinds"] = new JArray(kinds) }));
        }

        private Task<ToolResponse> HandleCreateFlowBlockAsync(JObject args, CancellationToken ct)
        {
            var type = ResolveType(args.Value<string>("typeFullName"));
            if (type == null || !typeof(BaseFlowBlock).IsAssignableFrom(type) || type.IsAbstract) return Task.FromResult(Fail("typeFullName invalid."));
            var reg = Reg(); var fb = reg.CreateFlowBlockUnregistered(type);
            if (!string.IsNullOrWhiteSpace(args.Value<string>("name"))) fb.Name = args.Value<string>("name");
            if (reg.GetFlowBlocks().Any(x => string.Equals(x.Name, fb.Name, StringComparison.OrdinalIgnoreCase))) return Task.FromResult(Fail($"FlowBlock name '{fb.Name}' already exists."));
            if (args.Value<int?>("x").HasValue && args.Value<int?>("y").HasValue) fb.Location = new System.Drawing.Point(args.Value<int>("x"), args.Value<int>("y"));
            reg.PostProcessFlowBlockCreated(fb); reg.RegisterFlowBlock(fb);
            return Task.FromResult(Ok(new JObject { ["created"] = true, ["name"] = fb.Name, ["typeFullName"] = fb.GetType().FullName }));
        }

        private Task<ToolResponse> HandleUpdateFlowBlockAsync(JObject args, CancellationToken ct)
        {
            var reg = Reg(); var name = args.Value<string>("name");
            var fb = reg.GetFlowBlocks().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(fb == null ? Fail($"FlowBlock '{name}' was not found.") : ApplyUpdates(args, fb, reg, "flowBlock", fb.Name));
        }

        private Task<ToolResponse> HandleDeleteFlowBlockAsync(JObject args, CancellationToken ct)
        {
            var reg = Reg(); var name = args.Value<string>("name");
            var fb = reg.GetFlowBlocks().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (fb == null) return Task.FromResult(Fail($"FlowBlock '{name}' was not found."));
            if (!fb.IsDeletable(out var deps)) return Task.FromResult(Fail($"FlowBlock '{name}' cannot be deleted. Dependencies: {string.Join(", ", deps.Select(x => x.Name))}"));
            reg.RemoveFlowBlock(fb); foreach (var other in reg.GetFlowBlocks()) if (other.ReferencedFlowBlocks.Contains(fb)) other.ReferencedFlowBlocks.Remove(fb);
            return Task.FromResult(Ok(new JObject { ["deleted"] = true, ["name"] = name }));
        }

        private Task<ToolResponse> HandleConnectFlowBlocksAsync(JObject args, CancellationToken ct) => ConnectDisconnect(args, connect: true);
        private Task<ToolResponse> HandleDisconnectFlowBlocksAsync(JObject args, CancellationToken ct) => ConnectDisconnect(args, connect: false);
        private Task<ToolResponse> ConnectDisconnect(JObject args, bool connect)
        {
            var reg = Reg(); var from = reg.GetFlowBlocks().FirstOrDefault(x => string.Equals(x.Name, args.Value<string>("from"), StringComparison.OrdinalIgnoreCase)); var to = reg.GetFlowBlocks().FirstOrDefault(x => string.Equals(x.Name, args.Value<string>("to"), StringComparison.OrdinalIgnoreCase));
            if (from == null || to == null) return Task.FromResult(Fail("from/to flow block was not found."));
            if (connect && !to.ReferencedFlowBlocks.Contains(from)) to.ReferencedFlowBlocks.Add(from);
            if (!connect && to.ReferencedFlowBlocks.Contains(from)) to.ReferencedFlowBlocks.Remove(from);
            return Task.FromResult(Ok(new JObject { [connect ? "connected" : "disconnected"] = true, ["from"] = from.Name, ["to"] = to.Name }));
        }

        private Task<ToolResponse> HandleCreateManagedObjectAsync(JObject args, CancellationToken ct)
        {
            var type = ResolveType(args.Value<string>("typeFullName"));
            if (type == null || !typeof(IManagedObject).IsAssignableFrom(type) || typeof(BaseFlowBlock).IsAssignableFrom(type) || type.IsAbstract) return Task.FromResult(Fail("typeFullName invalid."));
            var mo = (IManagedObject?)Activator.CreateInstance(type); if (mo == null) return Task.FromResult(Fail("Could not create managed object."));
            if (mo is FlowBloxComponent c && !string.IsNullOrWhiteSpace(args.Value<string>("name"))) c.Name = args.Value<string>("name");
            var reg = Reg(); reg.PostProcessManagedObjectCreated(mo); reg.RegisterManagedObject(mo);
            return Task.FromResult(Ok(new JObject { ["created"] = true, ["name"] = (mo as FlowBloxComponent)?.Name ?? string.Empty, ["typeFullName"] = mo.GetType().FullName }));
        }

        private Task<ToolResponse> HandleUpdateManagedObjectAsync(JObject args, CancellationToken ct)
        {
            var reg = Reg(); var name = args.Value<string>("name");
            var mo = reg.GetManagedObjects().OfType<FlowBloxComponent>().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(mo == null ? Fail($"Managed object '{name}' was not found.") : ApplyUpdates(args, mo, reg, "managedObject", mo.Name));
        }

        private Task<ToolResponse> HandleDeleteManagedObjectAsync(JObject args, CancellationToken ct)
        {
            var reg = Reg(); var name = args.Value<string>("name");
            var mo = reg.GetManagedObjects().OfType<FlowBloxComponent>().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (mo == null) return Task.FromResult(Fail($"Managed object '{name}' was not found."));
            if (!mo.IsDeletable(out var deps)) return Task.FromResult(Fail($"Managed object '{name}' cannot be deleted. Dependencies: {string.Join(", ", deps.Select(x => x.Name))}"));
            reg.RemoveManagedbject((IManagedObject)mo); return Task.FromResult(Ok(new JObject { ["deleted"] = true, ["name"] = name }));
        }

        private Task<ToolResponse> HandleGetManagedObjectsTypesAsync(JObject args, CancellationToken ct)
            => Task.FromResult(Ok(new JObject { ["managedObjectKinds"] = new JArray(Project().CreateInstances<IManagedObject>().Where(x => x is not BaseFlowBlock).GroupBy(x => x.GetType()).Select(g => ToTypeInfo((FlowBloxReactiveObject)Activator.CreateInstance(g.Key)!)).OrderBy(x => x.Value<string>("displayName"))) }));

        private Task<ToolResponse> HandleGetManagedObjectKindsInfoAsync(JObject args, CancellationToken ct)
            => Task.FromResult(TypeInfo(args.Value<string>("typeFullName"), typeof(IManagedObject), "managed object"));

        private Task<ToolResponse> HandleGetFlowBlockKindsInfoAsync(JObject args, CancellationToken ct)
            => Task.FromResult(TypeInfo(args.Value<string>("typeFullName"), typeof(BaseFlowBlock), "flow block"));

        private ToolResponse TypeInfo(string? fullName, Type mustAssign, string label)
        {
            var type = ResolveType(fullName);
            if (type == null || !mustAssign.IsAssignableFrom(type)) return Fail($"Type '{fullName}' is not a {label} type.");
            var instance = Activator.CreateInstance(type) as FlowBloxReactiveObject;
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.GetIndexParameters().Length == 0).Select(ToPropertyInfo).OrderBy(x => x.Value<string>("name"));
            return Ok(new JObject { ["kind"] = new JObject { ["fullName"] = type.FullName ?? type.Name, ["displayName"] = instance != null ? FlowBloxComponentHelper.GetDisplayName(instance) : type.Name, ["description"] = instance != null ? FlowBloxComponentHelper.GetDescription(instance) : string.Empty, ["properties"] = new JArray(props) } });
        }

        private Task<ToolResponse> HandleGetFlowBlockSnapshotAsync(JObject args, CancellationToken ct)
        {
            var fb = Reg().GetFlowBlocks().FirstOrDefault(x => string.Equals(x.Name, args.Value<string>("name"), StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(fb == null ? Fail($"FlowBlock '{args.Value<string>("name")}' was not found.") : Ok(Snapshot("flowBlock", fb.Name, fb.GetType().FullName, fb, fb.ReferencedFlowBlocks.Select(x => x.Name))));
        }

        private Task<ToolResponse> HandleGetManagedObjectSnapshotAsync(JObject args, CancellationToken ct)
        {
            var mo = Reg().GetManagedObjects().OfType<FlowBloxComponent>().FirstOrDefault(x => string.Equals(x.Name, args.Value<string>("name"), StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(mo == null ? Fail($"Managed object '{args.Value<string>("name")}' was not found.") : Ok(Snapshot("managedObject", mo.Name, mo.GetType().FullName, mo, null)));
        }

        private Task<ToolResponse> HandleGetFlowBloxComponentSnapshotAsync(JObject args, CancellationToken ct)
        {
            var kind = args.Value<string>("kind");
            return string.Equals(kind, "flowBlock", StringComparison.OrdinalIgnoreCase)
                ? HandleGetFlowBlockSnapshotAsync(new JObject { ["name"] = args.Value<string>("name") }, ct)
                : string.Equals(kind, "managedObject", StringComparison.OrdinalIgnoreCase)
                    ? HandleGetManagedObjectSnapshotAsync(new JObject { ["name"] = args.Value<string>("name") }, ct)
                    : Task.FromResult(Fail("kind must be 'flowBlock' or 'managedObject'."));
        }

        private async Task<ToolResponse> HandleBatchExecuteAsync(JObject args, CancellationToken ct)
        {
            if (args["requests"] is not JArray requests || !requests.Any()) return Fail("requests is required and must contain at least one entry.");
            var continueOnError = args.Value<bool?>("continueOnError") ?? false; var allOk = true; var arr = new JArray();
            foreach (var req in requests.OfType<JObject>())
            {
                var toolName = req.Value<string>("toolName") ?? string.Empty; var toolArgs = req["arguments"] as JObject ?? new JObject();
                if (string.IsNullOrWhiteSpace(toolName) || string.Equals(toolName, "BatchExecuteToolRequests", StringComparison.OrdinalIgnoreCase))
                {
                    allOk = false; arr.Add(new JObject { ["toolName"] = toolName, ["ok"] = false, ["error"] = string.IsNullOrWhiteSpace(toolName) ? "toolName is required." : "Nested batch not supported." });
                    if (!continueOnError) break; continue;
                }
                var r = await ExecuteAsync(new ToolRequest { ToolName = toolName, Arguments = toolArgs }, ct).ConfigureAwait(false);
                allOk &= r.Ok; arr.Add(new JObject { ["toolName"] = toolName, ["ok"] = r.Ok, ["error"] = r.Error, ["result"] = r.Result });
                if (!r.Ok && !continueOnError) break;
            }
            var payload = new JObject { ["allOk"] = allOk, ["batchResults"] = arr };
            return allOk ? Ok(payload) : Fail("One or more batch requests failed.", payload);
        }

        private ToolResponse ApplyUpdates(JObject args, object target, FlowBloxRegistry reg, string kind, string name)
        {
            var updates = new List<UpdateOperation>();
            if (!string.IsNullOrWhiteSpace(args.Value<string>("path")) && args["value"] != null)
            {
                updates.Add(new UpdateOperation { Path = args.Value<string>("path")!, Value = args["value"]! });
            }

            if (args["updates"] is JArray ua)
            {
                foreach (var u in ua.OfType<JObject>())
                {
                    if (!string.IsNullOrWhiteSpace(u.Value<string>("path")) && u["value"] != null)
                    {
                        updates.Add(new UpdateOperation { Path = u.Value<string>("path")!, Value = u["value"]! });
                    }
                }
            }
            if (!updates.Any()) return Fail("No update operations provided. Use path/value or updates[].");

            var applied = new JArray();
            foreach (var u in updates)
            {
                try { SetByJsonPath(target, u.Path, u.Value, reg); applied.Add(new JObject { ["path"] = u.Path, ["ok"] = true }); }
                catch (Exception ex) { applied.Add(new JObject { ["path"] = u.Path, ["ok"] = false, ["error"] = ex.Message }); return Fail($"Update failed at path '{u.Path}': {ex.Message}", new JObject { ["updated"] = false, ["kind"] = kind, ["name"] = name, ["applied"] = applied }); }
            }
            return Ok(new JObject { ["updated"] = true, ["kind"] = kind, ["name"] = name, ["applied"] = applied, ["count"] = applied.Count });
        }

        private static void SetByJsonPath(object root, string path, JToken value, FlowBloxRegistry reg)
        {
            var segs = ParsePath(path); if (!segs.Any()) throw new InvalidOperationException("Path is empty.");
            object current = root;
            for (var i = 0; i < segs.Count; i++)
            {
                var s = segs[i]; var last = i == segs.Count - 1;
                if (s.IsProperty)
                {
                    var pi = current.GetType().GetProperty(s.PropertyName!, BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{s.PropertyName}' not found on '{current.GetType().FullName}'.");
                    if (last) { pi.SetValue(current, ConvertToken(value, pi.PropertyType, reg)); return; }
                    var v = pi.GetValue(current) ?? CreateInstance(pi.PropertyType); if (pi.GetValue(current) == null) pi.SetValue(current, v); current = v;
                }
                else
                {
                    if (current is not IList list) throw new InvalidOperationException($"Index segment '{s.Index}' requires list-like target.");
                    var et = ElementType(current.GetType()) ?? throw new InvalidOperationException("Could not infer list element type.");
                    while (list.Count <= s.Index) list.Add(CreateInstance(et));
                    if (last) { list[s.Index] = ConvertToken(value, et, reg); return; }
                    var iv = list[s.Index] ?? CreateInstance(et); if (list[s.Index] == null) list[s.Index] = iv; current = iv;
                }
            }
        }

        private static List<PathSegment> ParsePath(string raw)
        {
            var p = raw?.Trim() ?? string.Empty; var segs = new List<PathSegment>(); if (string.IsNullOrWhiteSpace(p)) return segs;
            if (p.StartsWith('/')) { foreach (var x in p.Split('/', StringSplitOptions.RemoveEmptyEntries)) segs.Add(int.TryParse(x, out var idx) ? new PathSegment { IsProperty = false, Index = idx } : new PathSegment { IsProperty = true, PropertyName = x }); return segs; }
            foreach (var part in p.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var t = part.Trim(); var b = t.IndexOf('[');
                if (b < 0) { segs.Add(new PathSegment { IsProperty = true, PropertyName = t }); continue; }
                segs.Add(new PathSegment { IsProperty = true, PropertyName = t.Substring(0, b) });
                var rem = t.Substring(b);
                while (rem.StartsWith("[")) { var e = rem.IndexOf(']'); var txt = rem.Substring(1, e - 1); if (!int.TryParse(txt, out var idx)) throw new InvalidOperationException($"Invalid index '{txt}'."); segs.Add(new PathSegment { IsProperty = false, Index = idx }); rem = rem.Substring(e + 1); }
            }
            return segs;
        }

        private static object? ConvertToken(JToken token, Type t, FlowBloxRegistry reg)
        {
            if (token.Type == JTokenType.Null) return null;
            if (t == typeof(string)) return token.Value<string>();
            if (t.IsEnum) return Enum.Parse(t, token.Value<string>() ?? string.Empty, true);
            if (typeof(BaseFlowBlock).IsAssignableFrom(t) && token.Type == JTokenType.String) return reg.GetFlowBlocks().FirstOrDefault(x => string.Equals(x.Name, token.Value<string>(), StringComparison.OrdinalIgnoreCase));
            if (typeof(IManagedObject).IsAssignableFrom(t) && token.Type == JTokenType.String) return reg.GetManagedObjects().OfType<FlowBloxComponent>().FirstOrDefault(x => string.Equals(x.Name, token.Value<string>(), StringComparison.OrdinalIgnoreCase));
            return token.ToObject(t);
        }

        private static object CreateInstance(Type t)
        {
            if (t.IsInterface && t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(IList<>) || t.GetGenericTypeDefinition() == typeof(IEnumerable<>))) return Activator.CreateInstance(typeof(List<>).MakeGenericType(t.GetGenericArguments()[0]))!;
            if (t.IsValueType) return Activator.CreateInstance(t)!;
            return Activator.CreateInstance(t) ?? throw new InvalidOperationException($"Could not create instance of '{t.FullName}'.");
        }

        private static Type? ElementType(Type t)
        {
            if (t == typeof(string)) return null; if (t.IsArray) return t.GetElementType();
            if (t.IsGenericType && t.GetGenericArguments().Length == 1 && typeof(IEnumerable).IsAssignableFrom(t)) return t.GetGenericArguments()[0];
            var ie = t.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return ie?.GetGenericArguments()[0];
        }

        private static JObject Snapshot(string kind, string name, string? typeFullName, object root, IEnumerable<string>? refNames)
        {
            var shot = BuildSnapshotToken(root, new HashSet<object>(ReferenceEqualityComparer.Instance), 0) as JObject ?? new JObject();
            if (refNames != null) shot["ReferencedFlowBlocks"] = new JArray(refNames);
            return new JObject { ["kind"] = kind, ["name"] = name, ["typeFullName"] = typeFullName, ["snapshot"] = shot, ["snapshotJson"] = shot.ToString(Formatting.Indented) };
        }

        private static JToken BuildSnapshotToken(object? value, HashSet<object> visited, int depth)
        {
            if (value == null) return JValue.CreateNull();
            var t = value.GetType(); if (t == typeof(string) || t.IsEnum || t.IsPrimitive || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(Guid)) return JToken.FromObject(value);
            if (value is BaseFlowBlock fb) return fb.Name;
            if (value is IManagedObject mo) return mo is FlowBloxComponent c ? c.Name : mo.GetType().Name;
            if (depth > 8 || !visited.Add(value)) return new JObject { ["$ref"] = "circular-or-maxdepth" };
            if (value is IEnumerable en && value is not string) { var a = new JArray(); foreach (var i in en) a.Add(BuildSnapshotToken(i, visited, depth + 1)); return a; }
            if (value is FlowBloxReactiveObject ro)
            {
                var o = new JObject { ["$type"] = t.FullName ?? t.Name };
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.GetIndexParameters().Length == 0))
                {
                    try { o[p.Name] = BuildSnapshotToken(p.GetValue(ro), visited, depth + 1); } catch { }
                }
                return o;
            }
            return JToken.FromObject(value, JsonSerializer.Create(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.None }));
        }

        private static string[] PathOf(FlowBlockCategory c) { var s = new Stack<string>(); var x = c; while (x != null) { s.Push(x.DisplayName); x = x.ParentCategory; } return s.ToArray(); }
        private static Type? ResolveType(string? fullName) => string.IsNullOrWhiteSpace(fullName) ? null : ReflectionHelper.GetTypeByClass(fullName);

        private static JObject ToTypeInfo(FlowBloxReactiveObject x) => new() { ["fullName"] = x.GetType().FullName ?? x.GetType().Name, ["displayName"] = FlowBloxComponentHelper.GetDisplayName(x), ["description"] = FlowBloxComponentHelper.GetDescription(x) };
        private static JObject ToPropertyInfo(PropertyInfo p)
        {
            var d = p.GetCustomAttribute<DisplayAttribute>();
            var display = d != null ? FlowBloxResourceUtil.GetDisplayName(d, false) : p.Name;
            var desc = d != null ? FlowBloxResourceUtil.GetDescription(d) : string.Empty;
            var et = ElementType(p.PropertyType) ?? p.PropertyType;
            var simple = et.IsPrimitive || et.IsEnum || et == typeof(string) || et == typeof(decimal) || et == typeof(DateTime) || et == typeof(Guid);
            return new JObject { ["name"] = p.Name, ["displayName"] = string.IsNullOrWhiteSpace(display) ? p.Name : display, ["description"] = desc, ["type"] = p.PropertyType.FullName ?? p.PropertyType.Name, ["canWrite"] = p.CanWrite, ["isSimple"] = simple, ["isCollection"] = ElementType(p.PropertyType) != null, ["isReactiveObject"] = typeof(FlowBloxReactiveObject).IsAssignableFrom(et) };
        }

        private sealed class PathSegment
        {
            public bool IsProperty { get; set; }
            public string? PropertyName { get; set; }
            public int Index { get; set; }
        }

        private sealed class UpdateOperation
        {
            public string Path { get; set; } = string.Empty;
            public JToken Value { get; set; } = JValue.CreateNull();
        }
    }
}
