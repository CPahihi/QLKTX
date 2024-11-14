//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QLKTX.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DienNuoc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DienNuoc()
        {
            this.HoaDons = new HashSet<HoaDon>();
        }
    
        public int DienNuocID { get; set; }
        public Nullable<int> PhongID { get; set; }
        public Nullable<System.DateTime> TuNgay { get; set; }
        public Nullable<System.DateTime> DenNgay { get; set; }
        public Nullable<int> ChiSoDienCu { get; set; }
        public Nullable<int> ChiSoDienMoi { get; set; }
        public Nullable<int> ChiSoNuocCu { get; set; }
        public Nullable<int> ChiSoNuocMoi { get; set; }
        public Nullable<double> DonGiaDien { get; set; }
        public Nullable<double> DonGiaNuoc { get; set; }
    
        public virtual Phong Phong { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}