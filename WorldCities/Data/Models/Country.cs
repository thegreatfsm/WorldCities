using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorldCities.Data.Models
{
    public class Country
    {
        #region Constructor
        public Country()
        {

        }
        #endregion
        #region Properties
        [Key]
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        //Country code in ISO 3166-1 ALPHA-2 format
        [JsonPropertyName("iso2")]
        public string ISO2 { get; set; }
        //Country code in ISO 3166-1 ALPHA-3 format
        [JsonPropertyName("iso3")]
        public string ISO3 { get; set; }
        #endregion
        #region Navigation Properties
        public virtual List<City> Cities { get; set; }
        #endregion
    }
}
