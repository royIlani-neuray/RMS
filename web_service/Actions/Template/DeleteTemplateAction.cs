using WebService.Entites;
using WebService.Context;

namespace WebService.Actions.Template;

public class DeleteTemplateAction : RadarTemplateAction 
{
    public DeleteTemplateAction(string templateId) : base(templateId) {}

    protected override void RunTemplateAction(RadarTemplate radarTemplate)
    {
        System.Console.WriteLine($"Deleting radar template - {templateId}");
        TemplateContext.Instance.DeleteTemplate(radarTemplate);
    }
}