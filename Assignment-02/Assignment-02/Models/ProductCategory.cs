﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Assignment_02.Models;

/// <summary>
/// High-level product categorization.
/// </summary>
[Table("ProductCategory", Schema = "SalesLT")]
[Index("Name", Name = "AK_ProductCategory_Name", IsUnique = true)]
[Index("rowguid", Name = "AK_ProductCategory_rowguid", IsUnique = true)]
public partial class ProductCategory
{
    /// <summary>
    /// Primary key for ProductCategory records.
    /// </summary>
    [Key]
    public int ProductCategoryID { get; set; }

    /// <summary>
    /// Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.
    /// </summary>
    public int? ParentProductCategoryID { get; set; }

    /// <summary>
    /// Category description.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
    /// </summary>
    public Guid rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [InverseProperty("ParentProductCategory")]
    public virtual ICollection<ProductCategory> InverseParentProductCategory { get; set; } = new List<ProductCategory>();

    [ForeignKey("ParentProductCategoryID")]
    [InverseProperty("InverseParentProductCategory")]
    public virtual ProductCategory ParentProductCategory { get; set; }

    [InverseProperty("ProductCategory")]
    public virtual ICollection<Product> Product { get; set; } = new List<Product>();
}