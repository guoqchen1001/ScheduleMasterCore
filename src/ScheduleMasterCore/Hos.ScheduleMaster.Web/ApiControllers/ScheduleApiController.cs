using System;
using System.Linq;
using System.Threading.Tasks;
using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Dto;
using Hos.ScheduleMaster.Core.Interface;
using Hos.ScheduleMaster.Core.Models;
using Hos.ScheduleMaster.Web.Controllers;
using Hos.ScheduleMaster.Web.Extension;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hos.ScheduleMaster.Web.ApiControllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class ScheduleApiController: ControllerBase
    {
        [Autowired]
        public IScheduleService _scheduleService { get; set; }
        
       [HttpPost]
       public async Task<ServiceResponseMessage> Create(ScheduleInfo task)
        {
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
                return ApiResponse(ResultStatus.Illegal, e.Message);
            }
            
            
            if (result.Status != ResultStatus.Success) 
                return ApiResponse(ResultStatus.Failed, "创建任务失败");

            if (!task.RunNow) return ApiResponse(ResultStatus.Success, "任务创建成功！", 
                data: new {Id = result.Data});
            
            var start = await _scheduleService.Start(main);
            
            var resp = ApiResponse(ResultStatus.Success, 
                "任务创建成功！启动状态为：" + (start.Status == ResultStatus.Success ? "成功" : "失败"), 
                data: new {Id = result.Data});
            return resp;
        }

        [HttpPost]
        public ServiceResponseMessage Update(ScheduleInfo task)
        {
            var result = _scheduleService.Edit(task);
            result.Data = new {id = task.Id};
            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseMessage> Save(ScheduleInfo task)
        {
            var schedule = _scheduleService.QueryById(task.Id);
            if (schedule == null || schedule.Status == (int) ScheduleStatus.Deleted)
            {
                var resp =  await Create(task);
                return resp;
            }
            else
            {
                var resp = Update(task);
                return resp;
            }
               
            
            
        }
        
        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage Delete([FromBody] Guid id)
        {
            var task = _scheduleService.QueryById(id);
            
            return task == null || task.Status == (int) ScheduleStatus.Deleted ? 
                ApiResponse(ResultStatus.Success, "调度任务不存在或已删除，不允许删除!") : 
                _scheduleService.Delete(id);
        }

        /// <summary>
        /// 接口统一的返回消息
        /// </summary>
        /// <returns></returns>
        private ServiceResponseMessage ApiResponse(ResultStatus status, string message, object data = null)
        {
            return new ServiceResponseMessage(status, message, data);
        }
        
       

    }
}