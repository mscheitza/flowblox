using FlowBlox.Interfaces;
using System;
using System.Linq;
using System.Windows.Forms;

namespace FlowBlox.Services
{
    public class OwnerService : IOwnerService
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IWin32Window GetCurrentOwner()
        {
            var ownerForm = GetLastModalForm();
            if (ownerForm == null)
            {
                if (Application.OpenForms.Count > 0)
                    ownerForm = Application.OpenForms[0];
            }
            return ownerForm;
        }

        /// <summary>
        /// Retrieves the last modal form from the application.
        /// </summary>
        /// <returns>The last modal form or null if none found</returns>
        private Form GetLastModalForm()
        {
            var modalForms = Application.OpenForms.Cast<Form>()
                .Where(f => f.Modal && f.Visible)
                .ToList();

            return modalForms.LastOrDefault();
        }
    }
}
