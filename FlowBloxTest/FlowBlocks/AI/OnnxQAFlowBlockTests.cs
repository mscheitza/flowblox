using FlowBlox.Core.Models.FlowBlocks.AI;
using FlowBlox.Core.Models.FlowBlocks.AI.OnnxQA;
using Microsoft.ML.Tokenizers;

namespace FlowBloxTest.FlowBlocks.AI
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class OnnxQAFlowBlockTests
    {
        [TestMethod]
        public void KonstruktorVerwendetSichereQaStandardwerte()
        {
            var flowBlock = new OnnxQAFlowBlock();

            Assert.AreEqual(384, flowBlock.MaxSequenceLength);
            Assert.AreEqual(128, flowBlock.DocumentStride);
            Assert.AreEqual(30, flowBlock.MaxAnswerLength);
            Assert.IsFalse(flowBlock.AllowNoAnswer);
            Assert.AreEqual(0.0f, flowBlock.NoAnswerThreshold);
        }

        [TestMethod]
        public void SentencePieceOffsetsWerdenAufOriginalkontextAbgebildet()
        {
            const string original = "Tim lives in Sweden.";
            const string normalized = "▁Tim▁lives▁in▁Sweden.";
            var normalizedTokens = new[]
            {
                new EncodedToken(10, "▁Tim", new Range(0, 4)),
                new EncodedToken(11, "▁Swe", new Range(13, 17)),
                new EncodedToken(12, "den", new Range(17, 20)),
                new EncodedToken(13, ".", new Range(20, 21))
            };

            var mapped = OnnxQAModelTokenizer.MapSentencePieceOffsetsToOriginal(
                original,
                normalized,
                normalizedTokens);

            Assert.AreEqual("Tim", original[mapped[0].Offset]);
            Assert.AreEqual("Swe", original[mapped[1].Offset]);
            Assert.AreEqual("den", original[mapped[2].Offset]);
            Assert.AreEqual(".", original[mapped[3].Offset]);
        }

        [TestMethod]
        public void BesteAntwortspanneIgnoriertFrageUndSpezialtokens()
        {
            var contextTokens = new[]
            {
                new EncodedToken(10, "Alpha", new Range(0, 5)),
                new EncodedToken(11, "Beta", new Range(6, 10))
            };
            var window = new OnnxQAEncodedWindow(
                [1, 2, 3, 10, 11, 3],
                [1, 1, 1, 1, 1, 1],
                [0, 0, 0, 1, 1, 1],
                ContextStartIndex: 3,
                ContextTokenOffset: 0,
                ContextTokenCount: 2);
            var startLogits = new[] { 100f, 90f, 80f, 4f, 3f, 70f };
            var endLogits = new[] { 100f, 90f, 80f, 2f, 5f, 70f };

            var result = OnnxQASpanSelector.SelectBestSpan(
                startLogits,
                endLogits,
                window,
                contextTokens,
                maximumAnswerLength: 2);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.CharacterStart);
            Assert.AreEqual(10, result.CharacterEnd);
        }

        [TestMethod]
        public void BesteAntwortspanneBeachtetMaximaleAntwortlänge()
        {
            var contextTokens = new[]
            {
                new EncodedToken(10, "one", new Range(0, 3)),
                new EncodedToken(11, "two", new Range(4, 7)),
                new EncodedToken(12, "three", new Range(8, 13))
            };
            var window = new OnnxQAEncodedWindow(
                [1, 2, 10, 11, 12, 3],
                [1, 1, 1, 1, 1, 1],
                [0, 0, 1, 1, 1, 1],
                ContextStartIndex: 2,
                ContextTokenOffset: 0,
                ContextTokenCount: 3);
            var startLogits = new[] { 0f, 0f, 10f, -100f, -100f, 0f };
            var endLogits = new[] { 0f, 0f, 1f, 5f, 100f, 0f };

            var result = OnnxQASpanSelector.SelectBestSpan(
                startLogits,
                endLogits,
                window,
                contextTokens,
                maximumAnswerLength: 2);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.CharacterStart);
            Assert.AreEqual(7, result.CharacterEnd);
        }

        [TestMethod]
        public void WordPieceTokenizerBehältOffsetsInDenOriginalkontext()
        {
            var modelFolder = Path.Combine(Path.GetTempPath(), $"flowblox-onnx-qa-{Guid.NewGuid():N}");
            Directory.CreateDirectory(modelFolder);

            try
            {
                File.WriteAllLines(Path.Combine(modelFolder, "vocab.txt"),
                [
                    "[PAD]", "[UNK]", "[CLS]", "[SEP]", "[MASK]",
                    "When", "was", "Berlin", "founded", "?", "The", "city", "in", "1237", "."
                ]);
                File.WriteAllText(
                    Path.Combine(modelFolder, "tokenizer_config.json"),
                    """{"do_lower_case":false,"do_basic_tokenize":true,"strip_accents":false,"model_max_length":512}""");
                File.WriteAllText(
                    Path.Combine(modelFolder, "config.json"),
                    """{"model_type":"electra","max_position_embeddings":512}""");

                var tokenizer = OnnxQAModelTokenizer.Load(modelFolder);
                var questionTokens = tokenizer.EncodeTokens("When was Berlin founded?");
                var context = "The city was founded in 1237.";
                var contextTokens = tokenizer.EncodeTokens(context);
                var founded = contextTokens.Single(token => token.Value == "founded");
                var window = tokenizer.BuildWindow(questionTokens, contextTokens, 0, contextTokens.Count);

                Assert.AreEqual("founded", context[founded.Offset]);
                Assert.AreEqual(2, window.InputIds[0]);
                Assert.AreEqual(3, window.InputIds[window.ContextStartIndex - 1]);
                Assert.IsTrue(window.TokenTypeIds
                    .Skip(window.ContextStartIndex)
                    .Take(window.ContextTokenCount)
                    .All(value => value == 1));
            }
            finally
            {
                Directory.Delete(modelFolder, recursive: true);
            }
        }
    }
}
