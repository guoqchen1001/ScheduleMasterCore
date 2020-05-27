using System;
using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Dto;
using Hos.ScheduleMaster.Core.Interface;
using Hos.ScheduleMaster.Core.Models;
using Hos.ScheduleMaster.Web.Extension;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hos.ScheduleMaster.Web.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]")]
    public class ScheduleApiController: AdminController
    {
        [Autowired]
        public IScheduleService _scheduleService { get; set; }
        
       [HttpPost]
       [AllowAnonymous]
        public JsonNetResult Create(ScheduleInfo task)
        {
            var admin = CurrentAdmin;
            var main = new ScheduleEntity
            {
                MetaType = task.MetaType,
                CronExpression = task.CronExpression,
                EndDate = task.EndDate,
                Remark = task.Remark,
                StartDate = task.StartDate,
                Title = task.Title,
                Status = (int)ScheduleStatus.Stop,
                CustomParamsJson = task.CustomParamsJson,
                RunLoop = task.RunLoop,
                TotalRunCount = 0,
                CreateUserName = "admin"
            };
            if (task.MetaType == (int)ScheduleMetaType.Assembly)
            {
                main.AssemblyName = task.AssemblyName;
                main.ClassName = task.ClassName;
            }
            
            ScheduleHttpOptionEntity httpOption = null;
            if (task.MetaType == (int)ScheduleMetaType.Http)
            {
                httpOption = new ScheduleHttpOptionEntity
                {
                    RequestUrl = task.HttpRequestUrl,
                    Method = task.HttpMethod,
                    ContentType = task.HttpContentType,
                    Headers = task.HttpHeaders,
                    Body = task.HttpBody
                };
            }

            ServiceResponseMessage result;
            try
            {
                result = _scheduleService.Add(main, httpOption, task.Keepers, task.Nexts, task.Executors);
                
            }
            catch (Exception e)
            {
                return this.JsonNet(false, e.Message);
            }
            
            
            if (result.Status != ResultStatus.Success) return this.JsonNet(false);

            if (!task.RunNow) return this.JsonNet(true, "任务创建成功！", 
                data: new {Id = result.Data});
            
            var start = _scheduleService.Start(main);
            
            return this.JsonNet(true, 
                "任务创建成功！启动状态为：" + (start.Status == ResultStatus.Success ? "成功" : "失败"), 
                data: new {Id = result.Data});
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonNetResult Update(ScheduleInfo task)
        {
            var result = _scheduleService.Edit(task);
            
            return result.Status == ResultStatus.Success ? 
                this.JsonNet(true, "任务编辑成功", data: new {Id = task.Id}) : 
                this.JsonNet(false, $"任务编辑失败: {result.Message}",data: new {Id = task.Id});
        }
        
        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Delete(Guid id)
        {
            var result = _scheduleService.Delete(id);
            
            return this.JsonNet(result.Status == ResultStatus.Success, result.Message);
        }

    }
}