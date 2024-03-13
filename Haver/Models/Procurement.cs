﻿using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class Procurement
    {
        public int ID { get; set; }

        [Display(Name = "Does the supplier wants the items returned?")]
        [Required(ErrorMessage = "Choice is required.")]
        public bool SuppItemsBack { get; set; }

        [Display(Name = "RMA number")]
        public int? RMANo { get; set; } // LaterAdd - Only if SuppItemsBack True

        [Display(Name = "Carrier information")]
        [StringLength(3000, ErrorMessage = "The Carrier information is limited to 3000 characters.")]
        public string? CarrierInfo { get; set; } // LaterAdd - Only if SuppItemsBack True

        [Required(ErrorMessage = "Expected date of return required.")]
        [Display(Name = "When replaced/reworked items expected to be returned")]
        public DateTime ExpecDateOfReturn { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Choice is necessary.")]
        [Display(Name = "Has supplier return been completed in SAP?")]
        public bool SuppReturnCompleted { get; set; }

        [Required(ErrorMessage = "Choice is necessary.")]
        [Display(Name = "Is credit expected?")]
        public bool IsCreditExpec { get; set; }

        [Required(ErrorMessage = "Choice is necessary.")]
        [Display(Name = "Charge supplier for expenses?")]
        public bool ChargeSupplier { get; set; }

        [Required(ErrorMessage = "Procurement Date is required.")]
        [Display(Name = "Date")]
        public DateOnly ProcurementDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required(ErrorMessage = "Procurement is required.")]
        [Display(Name = "Procurement")]
        public string ProcurementSign { get; set; }

        [Display(Name = "Quality Photos")]
        public List<QualityPhoto> QualityPhotos { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }

    }
}
