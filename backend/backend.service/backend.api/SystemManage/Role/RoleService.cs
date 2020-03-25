﻿using AutoMapper;
using Backend.Entity.backend.api.Data;
using Backend.Jwtauthentication;
using Backend.Model.backend.api.Models.SystemManage;
using Backend.Repository.backend.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Service.backend.api.SystemManage.Role
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly SystemIdentityDbContext _systemIdentityDbContext;

        public RoleService(IRoleRepository roleRepository, IMapper mapper,
            SystemIdentityDbContext systemIdentityDbContext)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _systemIdentityDbContext = systemIdentityDbContext;
        }

        public async Task<string> AddRole(RoleViewModel model)
        {
            using (var trans = await _systemIdentityDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var role = _mapper.Map<SystemRole>(model);
                    var isSuccess = await _roleRepository.AddRole(role);
                    if (!isSuccess)
                    {
                        return "无法创建角色，请检查角色名是否相同！";
                    }
                    role = await _roleRepository.GetRole(model.Name);
                    isSuccess = await _roleRepository.AddOrUpdateRoleClaim(role,
                        CustomClaimTypes.RoleMenus, model.Menus);
                    if (!isSuccess)
                    {
                        await trans.RollbackAsync();
                        return "无法创建角色，菜单权限添加失败！";
                    }
                    await trans.CommitAsync();
                }
                catch (Exception e)
                {
                    await trans.RollbackAsync();
                    throw e;
                }
            }
            return string.Empty;
        }

        public int Count()
        {
            return _roleRepository.GetRoles().Count();
        }

        public async Task<RoleResult> GetRole(string id)
        {
            var role = await _roleRepository.GetRoleById(id);
            var roleResult = _mapper.Map<RoleResult>(role);
            roleResult.Menus = await _roleRepository.GetRoleClaimValue(role, CustomClaimTypes.RoleMenus);
            return roleResult;
        }

        public IEnumerable<RoleResult> GetRoles(int page, int size, string name)
        {
            var roles = string.IsNullOrEmpty(name) ?
                _roleRepository.GetRoles(page, size).ToArray() :
                _roleRepository.GetRoles(role => role.Name.Contains(name), page, size).ToArray();
            return _mapper.Map<SystemRole[], IEnumerable<RoleResult>>(roles);
        }

        public IEnumerable<string> GetRoles()
        {
            return _roleRepository.GetRoles().Select(r => r.Name);
        }

        public async Task<string> RemoveRole(string roleName)
        {
            var role = await _roleRepository.GetRole(roleName);
            if (role == null)
            {
                return "无法删除角色，角色不存在！";
            }
            else if (role.Name == "超级管理员")
            {
                return "系统超级管理员,不可删除修改!";
            }
            var count = await _roleRepository.GetMemberCount(roleName);
            if (count > 0)
            {
                return "无法删除角色，角色中包含用户！";
            }
            var isSuccess = await _roleRepository.RemoveRole(roleName);
            if (!isSuccess)
            {
                return "角色删除失败！";
            }
            return string.Empty;
        }

        public async Task<string> Update(RoleViewModel model)
        {
            using (var trans = await _systemIdentityDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var role = await _roleRepository.GetRole(model.Name);
                    _mapper.Map(model, role);
                    var isSuccess = await _roleRepository.UpdateRole(role);
                    if (!isSuccess)
                    {
                        return "无法更新角色，请检查角色名是否相同！";
                    }
                    role = await _roleRepository.GetRole(model.Name);
                    isSuccess = await _roleRepository.AddOrUpdateRoleClaim(role,
                        CustomClaimTypes.RoleMenus, model.Menus);
                    if (!isSuccess)
                    {
                        await trans.RollbackAsync();
                        return "无法更新角色，菜单权限添加失败！";
                    }
                    await trans.CommitAsync();
                }
                catch (Exception e)
                {
                    await trans.RollbackAsync();
                    throw e;
                }
            }
            return string.Empty;
        }


    }
}
