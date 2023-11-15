﻿using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class TokenModel
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
