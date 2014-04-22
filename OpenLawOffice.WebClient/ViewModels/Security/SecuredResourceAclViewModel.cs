﻿// -----------------------------------------------------------------------
// <copyright file="SecuredResourceAclViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.WebClient.ViewModels.Security
{
    using System;
    using AutoMapper;
    using OpenLawOffice.Common.Models;

    [MapMe]
    public class SecuredResourceAclViewModel : CoreViewModel
    {
        public Guid? Id { get; set; }

        public SecuredResourceViewModel SecuredResource { get; set; }

        public UserViewModel User { get; set; }

        public PermissionsViewModel AllowPermissions { get; set; }

        public PermissionsViewModel DenyPermissions { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Security.SecuredResourceAcl, SecuredResourceAclViewModel>()
                .ForMember(dst => dst.IsStub, opt => opt.UseValue(false))
                .ForMember(dst => dst.UtcCreated, opt => opt.MapFrom(src => src.UtcCreated))
                .ForMember(dst => dst.UtcModified, opt => opt.MapFrom(src => src.UtcModified))
                .ForMember(dst => dst.UtcDisabled, opt => opt.MapFrom(src => src.UtcDisabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Security.UserViewModel()
                    {
                        Id = db.CreatedBy.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Security.UserViewModel()
                    {
                        Id = db.ModifiedBy.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(db =>
                {
                    if (db.DisabledBy == null || !db.DisabledBy.Id.HasValue) return null;
                    return new ViewModels.Security.UserViewModel()
                    {
                        Id = db.DisabledBy.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.SecuredResource, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Security.SecuredResourceViewModel()
                    {
                        Id = db.SecuredResource.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.User, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Security.UserViewModel()
                    {
                        Id = db.User.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.AllowPermissions, opt => opt.ResolveUsing(model =>
                {
                    return (PermissionsViewModel)(int)model.AllowFlags.Value;
                }))
                .ForMember(dst => dst.DenyPermissions, opt => opt.ResolveUsing(model =>
                {
                    return (PermissionsViewModel)(int)model.DenyFlags.Value;
                }));

            Mapper.CreateMap<SecuredResourceAclViewModel, Common.Models.Security.SecuredResourceAcl>()
                .ForMember(dst => dst.UtcCreated, opt => opt.MapFrom(src => src.UtcCreated))
                .ForMember(dst => dst.UtcModified, opt => opt.MapFrom(src => src.UtcModified))
                .ForMember(dst => dst.UtcDisabled, opt => opt.MapFrom(src => src.UtcDisabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(model =>
                {
                    if (model.CreatedBy == null || !model.CreatedBy.Id.HasValue)
                        return null;
                    return new Common.Models.Security.User()
                    {
                        Id = model.CreatedBy.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(model =>
                {
                    if (model.ModifiedBy == null || !model.ModifiedBy.Id.HasValue)
                        return null;
                    return new Common.Models.Security.User()
                    {
                        Id = model.ModifiedBy.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(model =>
                {
                    if (model.DisabledBy == null || !model.DisabledBy.Id.HasValue)
                        return null;
                    return new Common.Models.Security.User()
                    {
                        Id = model.DisabledBy.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.SecuredResource, opt => opt.ResolveUsing(model =>
                {
                    return new Common.Models.Security.SecuredResource()
                    {
                        Id = model.SecuredResource.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.User, opt => opt.ResolveUsing(model =>
                {
                    return new Common.Models.Security.User()
                    {
                        Id = model.User.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.AllowFlags, opt => opt.ResolveUsing(model =>
                {
                    return (int)model.AllowPermissions;
                }))
                .ForMember(dst => dst.DenyFlags, opt => opt.ResolveUsing(model =>
                {
                    return (int)model.DenyPermissions;
                }));
        }
    }
}