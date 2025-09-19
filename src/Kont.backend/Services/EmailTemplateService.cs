using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Kont.backend.Models.Email;

namespace Kont.backend.Services;

public interface IEmailTemplateService
{
    Task<string> RenderEventInvitationTemplateAsync(EventInvitationEmailModel model);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ICompositeViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public EmailTemplateService(
        ICompositeViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderEventInvitationTemplateAsync(EventInvitationEmailModel model)
    {
        var viewName = "EventInvitation";
        var viewPath = $"~/Views/EmailTemplates/{viewName}.cshtml";

        var actionContext = new ActionContext(
            new DefaultHttpContext { RequestServices = _serviceProvider },
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        var viewResult = _viewEngine.FindView(actionContext, viewPath, false);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"View '{viewPath}' not found");
        }

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

        using var writer = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
