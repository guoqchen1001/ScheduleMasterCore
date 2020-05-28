﻿using Hos.ScheduleMaster.Core.Common;
using Hos.ScheduleMaster.Core.Interface;
using Hos.ScheduleMaster.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hos.ScheduleMaster.Core.Services
{
    [ServiceMapTo(typeof(ISystemService))]
    public class SystemService : BaseService, ISystemService
    {
        /// <summary>
        /// 查询所有配置项
        /// </summary>
        /// <returns></returns>
        public List<SystemConfigEntity> GetConfigList()
        {
            return _repositoryFactory.SystemConfigs.Table.ToList();
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool SaveConfig(Dictionary<string, string> items, string updateUserName)
        {
            foreach (var item in items)
            {
                _repositoryFactory.SystemConfigs.UpdateBy(
                    x => x.Key == item.Key,
                    new string[] { "Value", "UpdateTime", "UpdateUserName" },
                    new object[] { items[item.Key], DateTime.Now, updateUserName });
            }
            if (items.Any())
            {
                return _unitOfWork.Commit() > 0;
            }
            return true;
        }

        /// <summary>
        /// 查询日志分页数据
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        public ListPager<SystemLogEntity> QueryLogPager(ListPager<SystemLogEntity> pager)
        {
            return _repositoryFactory.SystemLogs.WherePager(pager, m => true, m => m.CreateTime, false);
        }

        /// <summary>
        /// 根据条件删除日志
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="category"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public int DeleteLog(Guid? sid, int? category, DateTime? startdate, DateTime? enddate)
        {
            IQueryable<SystemLogEntity> query = _repositoryFactory.SystemLogs.Table;
            if (sid.HasValue)
            {
                query = query.Where(x => x.ScheduleId == sid.Value);
            }
            if (category.HasValue)
            {
                query = query.Where(x => x.Category == category.Value);
            }
            if (startdate.HasValue)
            {
                query = query.Where(x => x.CreateTime >= startdate.Value);
            }
            if (enddate.HasValue)
            {
                query = query.Where(x => x.CreateTime < enddate.Value);
            }
            _repositoryFactory.SystemLogs.DeleteBy(query);
            return _unitOfWork.Commit();
        }

    }
}
