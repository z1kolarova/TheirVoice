using System.Collections.Generic;
using System.Linq;

public static class DBServiceUtils
{    
    public static IQueryable<Prompt> GetSystemPrompts()
        => DBService.I.Prompts.Where(p => p.IsSystemPrompt);

    public static bool LangHasAllSystemPrompts(int langId)
    {
        return DBServiceUtils.GetSystemPrompts().All(sp =>
            DBService.I.PromptLocs.Any(pl 
                => pl.LangId == langId 
                && pl.PromptId == sp.Id
                && pl.Available)
        );
    }

    public static IEnumerable<Language> GetLanguagesWithAvailablePromptLocs()
    {
        var languagesWithValidSystemPrompts = DBService.I.Languages
            .Where(l => LangHasAllSystemPrompts(l.Id))
            .ToList();

        var validNonSystemPrompts = DBService.I.Prompts
            .Where(p => p.ActiveIfAvailable && !p.Name.StartsWith("_"))
            .ToList();

        var result = languagesWithValidSystemPrompts.Where(l 
            => DBService.I.PromptLocs.Any(pl 
                => validNonSystemPrompts.Any(vp 
                    => pl.LangId == l.Id && pl.PromptId == vp.Id)));

        return result;
    }

    public static IEnumerable<Prompt> GetReadyToUseNonSystemPromptsInLanguage(int langId)
    {
        var validPrompts = DBService.I.Prompts.Where(p => p.ActiveIfAvailable
            && !p.IsSystemPrompt
            && DBService.I.PromptLocs.Any(pl 
                => pl.PromptId == p.Id 
                && pl.LangId == langId 
                && pl.Available))
            .ToList();

        return validPrompts;
    }

    public static string GetPromptLocText(int promptId, int langId)
    {
        var promptLoc = DBService.I.PromptLocs.FirstOrDefault(pl
            => pl.PromptId == promptId
            && pl.LangId == langId
            && pl.Available);

        return promptLoc?.Text ?? $"Something went wrong getting PromptLoc text for prompt with Id {promptId} and language with Id {langId}.";
    }
}
