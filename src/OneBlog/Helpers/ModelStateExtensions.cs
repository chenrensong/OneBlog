using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OneBlog.Helpers
{
    public static class ModelStateExtensions
    {
        //public static IEnumerable<Error> AllErrors(this ModelStateDictionary modelState)
        //{
        //    var result = new List<Error>();
        //    var erroneousFields = modelState.Where(ms => ms.Value.Errors.Any())
        //                                    .Select(x => new { x.Key, x.Value.Errors });

        //    foreach (var erroneousField in erroneousFields)
        //    {
        //        var fieldKey = erroneousField.Key;
        //        var fieldErrors = erroneousField.Errors
        //                           .Select(error => new Error(fieldKey, error.ErrorMessage));
        //        result.AddRange(fieldErrors);
        //    }

        //    return result;
        //}

        public static IEnumerable<Error> AllErrors(this ModelStateDictionary modelState)
        {
            var result = from ms in modelState
                         where ms.Value.Errors.Any()
                         let fieldKey = ms.Key
                         let errors = ms.Value.Errors
                         from error in errors
                         select new Error(fieldKey, error.ErrorMessage);

            return result;
        }
    }

    public class Error
    {
        public Error(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; set; }
        public string Message { get; set; }
    }
}
