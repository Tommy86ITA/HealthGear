using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HealthGear.Helpers;

public static class ControllerExtensions
{
    public static async Task<string> RenderViewToStringAsync(this Controller controller, string viewName, object model)
    {
        controller.ViewData.Model = model;

        using var writer = new StringWriter();
        var viewEngine =
            controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
        var view = viewEngine.FindView(controller.ControllerContext, viewName, false);

        if (view.View == null) throw new ArgumentNullException($"View {viewName} not found.");

        var viewContext = new ViewContext(
            controller.ControllerContext,
            view.View,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions()
        );

        await view.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}