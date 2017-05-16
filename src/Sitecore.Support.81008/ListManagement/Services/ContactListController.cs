using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.ListManagement.Configuration;
using Sitecore.ListManagement.ContentSearch.Model;
using Sitecore.ListManagement.Services;
using Sitecore.ListManagement.Services.Model;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Sitecore.Services;

namespace Sitecore.Support.ListManagement.Services
{
    [ServicesController("ListManagement.SupportContactList"), AnalyticsDisabledAttributeFilter, ContactListLockedExceptionFilter, AccessDeniedExceptionFilter, UnauthorizedAccessExceptionFilter, SitecoreAuthorize(Roles = @"sitecore\List Manager Editors")]
    public class SupportContactListController : EntityService<ContactListModel>
    {
        private readonly ContactListRepository<ContactList, ContactList, ContactListModel> repository;
        private static Database listManagementDatabase = Factory.GetDatabase(ListManagementSettings.Database);

        protected virtual IEnumerable<ContactListModel> ApplySecurity(IEnumerable<ContactListModel> contactLists) =>
            (from l in contactLists
             where listManagementDatabase.GetItem(new ID(l.itemId)) != null
             select l);

        //public ContactListController() : this (Sitecore.Configuration.Factory.CreateObject("/sitecore/contactRepository", true) as ContactListRepository<ContactList, ContactList, ContactListModel>)
        //{
        //}

        public SupportContactListController(ContactListRepository<ContactList, ContactList, ContactListModel> repository)
            : base(repository)
        {
            this.repository = repository;
        }

        [HttpGet, ActionName("DefaultAction")]
        public ContactListModel[] FetchEntities(string rootId, string filter, string searchExpression, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(rootId))
            {
                rootId = ListManagementSettings.ListsRootId.ToString();
            }
            bool flag = this.IsRecursiveWithoutFolders(filter);
            bool includeListsFromSubfolders = flag || !string.IsNullOrEmpty(searchExpression);
            bool includeFolders = !flag && string.IsNullOrEmpty(searchExpression);
            return ApplySecurity(this.repository.GetFilteredContactListsFromPathWithCount(rootId, filter, searchExpression, pageIndex, pageSize, includeFolders, includeListsFromSubfolders)).ToArray<ContactListModel>();
        }

        protected virtual bool IsRecursiveWithoutFolders(string filter)
        {
            if (filter != "getRecentlyCreatedLists")
            {
                return (filter == "getMyLists_Dashboard");
            }
            return true;
        }
    }
}
