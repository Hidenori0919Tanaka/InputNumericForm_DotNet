using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NumericInputForm.Models
{
    public class NumericModel
    {
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyy/MM/dd}")]
        public DateTime Date { get; set; }
        public float Numeric { get; set; }
        public bool InterpolationFlag { get; set; }
        public bool InputFlag { get; set; }
        public bool OutputFlag { get; set; }
    }
}