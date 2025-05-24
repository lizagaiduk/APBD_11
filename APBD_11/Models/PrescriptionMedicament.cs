using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tutorial11.Models;

[Table("Prescription_Medicament")]
[PrimaryKey(nameof(IdPrescription), nameof(IdMedicament))]
public class PrescriptionMedicament
{
    [ForeignKey(nameof(Prescription))]
    public int IdPrescription { get; set; }

    [ForeignKey(nameof(Medicament))]
    public int IdMedicament { get; set; }

    public int Dose { get; set; }

    [MaxLength(300)]
    public string Description { get; set; }

    public Prescription Prescription { get; set; }
    public Medicament Medicament { get; set; }
}