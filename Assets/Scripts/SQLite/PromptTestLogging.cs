using Assets.Interfaces;
using SQLite4Unity3d;
using System;
using System.Linq.Expressions;

public class PromptTestLogging : IHasPrimaryKey, IPseudoCompositeKey<PromptTestLogging>
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PromptId { get; set; }
    public int LangId { get; set; }

    public bool FullLogNextTime {  get; set; }

    public object GetPrimaryKey()
        => Id;
    public Expression<Func<PromptTestLogging, bool>> GetCompositeKeyPredicate()
        => ptl 
            => ptl.PromptId == PromptId 
            && ptl.LangId == LangId;
    public override string ToString()
        => $"[{nameof(PromptLoc)}: {nameof(PromptId)}={PromptId}" +
            $", {nameof(LangId)}={LangId}" +
            $", {nameof(FullLogNextTime)}={FullLogNextTime}" +
            $"]";
}