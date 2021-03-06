﻿// -----------------------------------------------------------------------
// <copyright file="ContactsController.cs" company="Nodine Legal, LLC">
// Licensed to Nodine Legal, LLC under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  Nodine Legal, LLC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenLawOffice.WebClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using AutoMapper;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class ContactsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            List<ViewModels.Contacts.ContactViewModel> viewModelList = null;
            string contactFilter;


            viewModelList = new List<ViewModels.Contacts.ContactViewModel>();

            if (!string.IsNullOrEmpty(contactFilter = Request["contactFilter"]))
            {
                Data.Contacts.Contact.List(contactFilter).ForEach(x =>
                {
                    viewModelList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }
            else
            {
                Data.Contacts.Contact.List().ForEach(x =>
                {
                    viewModelList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }

            return View(viewModelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult ListDisplayNameOnly()
        {
            string term;
            List<Common.Models.Contacts.Contact> list;

            term = Request["term"];

            list = Data.Contacts.Contact.List(term.Trim());

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        private List<ViewModels.Contacts.ContactViewModel> GetList()
        {
            List<ViewModels.Contacts.ContactViewModel> modelList = null;

            modelList = new List<ViewModels.Contacts.ContactViewModel>();

            OpenLawOffice.Data.Contacts.Contact.List().ForEach(x =>
            {
                modelList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
            });

            return modelList;
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            Common.Models.Contacts.Contact contact = null;
            ViewModels.Contacts.ContactViewModel viewModel;

            contact = OpenLawOffice.Data.Contacts.Contact.Get(id);
            if (contact.BillingRate != null && contact.BillingRate.Id.HasValue)
                contact.BillingRate = Data.Billing.BillingRate.Get(contact.BillingRate.Id.Value);
            else
                contact.BillingRate = null;

            if (contact == null)
                return View("InvalidRequest");

            viewModel = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);
            if (contact.BillingRate != null)
                viewModel.BillingRate = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(contact.BillingRate);

            PopulateCoreDetails(viewModel);

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Timesheets(int id)
        {
            DateTime now = DateTime.Now;
            DateTime? from = null, to = null;

            ViewModels.Contacts.TimesheetsViewModel viewModel = new ViewModels.Contacts.TimesheetsViewModel();
            
            viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(OpenLawOffice.Data.Contacts.Contact.Get(id));

            if (!string.IsNullOrEmpty(Request["From"]))
                from = DateTime.Parse(Request["From"]);
            if (!string.IsNullOrEmpty(Request["To"]))
                to = DateTime.Parse(Request["To"]);

            if (!from.HasValue)
                from = new DateTime(now.Year, now.Month, 1);
            if (!to.HasValue)
                to = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);

            if (from.HasValue)
                ViewData["From"] = from.Value;
            if (to.HasValue)
                ViewData["To"] = to.Value;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Timesheets_Print3rdParty(int id)
        {
            DateTime now = DateTime.Now;
            int contactId = id;
            DateTime? from = null, to = null;
            ViewModels.Contacts.TimesheetsViewModel viewModel = new ViewModels.Contacts.TimesheetsViewModel();
            
            if (!string.IsNullOrEmpty(Request["From"]))
                from = DateTime.Parse(Request["From"]);
            if (!string.IsNullOrEmpty(Request["To"]))
                to = DateTime.Parse(Request["To"]);

            if (!from.HasValue)
                from = new DateTime(now.Year, now.Month, 1);
            if (!to.HasValue)
                to = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);

            viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(Data.Contacts.Contact.Get(id));

            // Method to get all matters for which BillTo is set to this contact
            Data.Contacts.Contact.ListMattersWhereContactIsBillTo(contactId).ForEach(matter =>
            {
                ViewModels.Contacts.TimesheetsViewModel.MatterTimeList mtl = new ViewModels.Contacts.TimesheetsViewModel.MatterTimeList();
                ViewModels.Contacts.TimesheetsViewModel.TimeItem timeItem;
                
                mtl.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(matter);
                Data.Timing.Time.ListForMatterWithinRange(matter.Id.Value, from, to).ForEach(time =>
                {
                    timeItem = new ViewModels.Contacts.TimesheetsViewModel.TimeItem();

                    timeItem.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(time);

                    timeItem.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(
                        Data.Timing.Time.GetRelatedTask(timeItem.Time.Id.Value));

                    timeItem.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Tasks.Task.GetRelatedMatter(timeItem.Task.Id.Value));

                    mtl.Times.Add(timeItem);
                });

                viewModel.Matters.Add(mtl);
            });

            if (from.HasValue)
                ViewData["From"] = from.Value;
            if (to.HasValue)
                ViewData["To"] = to.Value;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create()
        {
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;

            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();

            Data.Billing.BillingRate.List().ForEach(x =>
            {
                ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                billingRateList.Add(vm);
            });

            ViewData["BillingRateList"] = billingRateList;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Contacts.ContactViewModel viewModel)
        {
            string errorListString = "";
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;
            Common.Models.Account.Users currentUser = null;
            Common.Models.Contacts.Contact model;
            List<Common.Models.Contacts.Contact> possibleDuplicateList;

            currentUser = Data.Account.Users.Get(User.Identity.Name);

            model = Mapper.Map<Common.Models.Contacts.Contact>(viewModel);

            if (Request["OverrideConflict"] != "True")
            {
                possibleDuplicateList = Data.Contacts.Contact.ListPossibleDuplicates(model);

                if (possibleDuplicateList.Count > 0)
                {
                    billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();

                    possibleDuplicateList.ForEach(x =>
                    {
                        errorListString += "<li><a href=\"/Contacts/Details/" + x.Id.Value + "\">" + x.DisplayName + "</a> [<a href=\"/Contacts/Edit/" + x.Id.Value + "\">edit</a>]</li>";
                    });

                    Data.Billing.BillingRate.List().ForEach(x =>
                    {
                        ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                        vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                        billingRateList.Add(vm);
                    });

                    ViewData["ErrorMessage"] = "Contact possibly conflicts with the following existing contacts:<ul>" + errorListString + "</ul>Click Save again to create the contact anyway.";
                    ViewData["OverrideConflict"] = "True";
                    ViewData["BillingRateList"] = billingRateList;
                    return View(viewModel);
                }
            }
            
            model = OpenLawOffice.Data.Contacts.Contact.Create(model, currentUser);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            Common.Models.Contacts.Contact model = null;
            ViewModels.Contacts.ContactViewModel viewModel;
            List<ViewModels.Billing.BillingRateViewModel> billingRateList;

            billingRateList = new List<ViewModels.Billing.BillingRateViewModel>();

            model = OpenLawOffice.Data.Contacts.Contact.Get(id);

            viewModel = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model);

            if (model.BillingRate != null && model.BillingRate.Id.HasValue)
            {
                model.BillingRate = Data.Billing.BillingRate.Get(model.BillingRate.Id.Value);
                viewModel.BillingRate = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(model.BillingRate);
            }

            Data.Billing.BillingRate.List().ForEach(x =>
            {
                ViewModels.Billing.BillingRateViewModel vm = Mapper.Map<ViewModels.Billing.BillingRateViewModel>(x);
                vm.Title += " (" + vm.PricePerUnit.ToString("C") + ")";
                billingRateList.Add(vm);
            });

            ViewData["BillingRateList"] = billingRateList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Contacts.ContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser = null;
            Common.Models.Contacts.Contact model;

            currentUser = Data.Account.Users.Get(User.Identity.Name);

            model = Mapper.Map<Common.Models.Contacts.Contact>(viewModel);

            model = OpenLawOffice.Data.Contacts.Contact.Edit(model, currentUser);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Conflicts(int id)
        {
            List<Common.Models.Matters.Matter> matterList = null;
            Common.Models.Contacts.Contact contact;
            List<Tuple<Common.Models.Matters.Matter, Common.Models.Matters.MatterContact, Common.Models.Contacts.Contact>> matterRelationshipList;
            ViewModels.Contacts.ConflictViewModel viewModel = new ViewModels.Contacts.ConflictViewModel();

            contact = Data.Contacts.Contact.Get(id);

            viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);

            viewModel.Matters = new List<ViewModels.Contacts.ConflictViewModel.MatterRelationship>();

            matterList = Data.Contacts.Contact.ListMattersForContact(id);

            foreach (var x in matterList)
            {
                ViewModels.Contacts.ConflictViewModel.MatterRelationship mr = new ViewModels.Contacts.ConflictViewModel.MatterRelationship();

                mr.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(x);

                mr.MatterContacts = new List<ViewModels.Matters.MatterContactViewModel>();

                matterRelationshipList = Data.Contacts.Contact.ListMatterRelationshipsForContact(id, x.Id.Value);

                matterRelationshipList.ForEach(y =>
                {
                    ViewModels.Matters.MatterContactViewModel mc = Mapper.Map<ViewModels.Matters.MatterContactViewModel>(y.Item2);
                    mc.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(y.Item3);
                    mc.Matter = mr.Matter;
                    mr.MatterContacts.Add(mc);
                });

                viewModel.Matters.Add(mr);
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Matters(int? id)
        {
            List<ViewModels.Matters.MatterViewModel> list;
            int contactId;

            if (id.HasValue)
                contactId = id.Value;
            else
                contactId = int.Parse(Request["ContactId"]);

            list = new List<ViewModels.Matters.MatterViewModel>();
            Data.Matters.Matter.ListAllMattersForContact(contactId).ForEach(x =>
            {
                list.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
            });

            return View(list);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Tasks(int? id)
        {
            ViewModels.Tasks.TaskViewModel viewModel;
            List<ViewModels.Tasks.TaskViewModel> list;
            int contactId;

            if (id.HasValue)
                contactId = id.Value;
            else
                contactId = int.Parse(Request["ContactId"]);

            list = new List<ViewModels.Tasks.TaskViewModel>();
            Data.Tasks.Task.ListAllTasksForContact(contactId).ForEach(x =>
            {
                viewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(x);

                if (viewModel.IsGroupingTask)
                {
                    if (Data.Tasks.Task.GetTaskForWhichIAmTheSequentialPredecessor(x.Id.Value) != null)
                        viewModel.Type = "Sequential Group";
                    else
                        viewModel.Type = "Group";
                }
                else
                {
                    if (x.SequentialPredecessor != null)
                        viewModel.Type = "Sequential";
                    else
                        viewModel.Type = "Standard";
                }
                list.Add(viewModel);
            });

            return View(list);
        }
    }
}