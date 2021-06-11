using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OPS.Business;
using OPS.Utility;
using OPS.ViewModels.Admin.Master.Event;
using OPS.ViewModels.Shared;
using System;
using System.Linq;
using System.Security.Claims;

namespace OPS.ViewComponents.Shared
{
    public class LayoutHeaderViewComponent : ViewComponent
    {
        private IUnitOfWork _unitOfWork { get; set; }

        public LayoutHeaderViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IViewComponentResult Invoke()
        {
            var eventCookie = new EventCookieModel();
            var eventInfo = new EventHeaderInfo();

            var cookie = HttpContext.Request.Cookies.Where(q => q.Key.Contains(Constants.EventCookie)).LastOrDefault();
            if (!string.IsNullOrEmpty(cookie.Value))
            {
                eventCookie = JsonConvert.DeserializeObject<EventCookieModel>(cookie.Value);
            }
            else
            {
                var eventCode = HttpContext.User.FindFirstValue(ClaimTypes.Name);
                var eventLoadedInfo = _unitOfWork.MstEventRepository.LoadEventInfoByCode(eventCode);
                HttpContext.Response.Cookies.Append(Constants.EventCookie, eventLoadedInfo.Message, new CookieOptions { Expires = DateTimeOffset.Now.AddMonths(1) });

                eventCookie = JsonConvert.DeserializeObject<EventCookieModel>(eventLoadedInfo.Message);
            }

            eventInfo.EventName = eventCookie.Name;
            eventInfo.Eventdate = eventCookie.EventTime;

            return View("_LayoutHeaderPartial", eventInfo);
        }
    }
}
