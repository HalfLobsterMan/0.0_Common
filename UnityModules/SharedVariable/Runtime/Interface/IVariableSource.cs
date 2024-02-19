#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion
using System.Collections.Generic;

namespace CZToolKit.SharedVariable
{
    public interface IVariableSource
    {
        IVariableOwner VarialbeOwner { get; }

        IReadOnlyList<SharedVariable> Variables { get; }
    }
}