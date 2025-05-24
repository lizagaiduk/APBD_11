using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutorial11.Models;

[Table("Prescription")]
public class Prescription
{
    [Key]
    public int IdPrescription { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime DueDate { get; set; }

    public int IdPatient { get; set; }
    [ForeignKey("IdPatient")]
    public Patient Patient { get; set; }

    public int IdDoctor { get; set; }
    [ForeignKey("IdDoctor")]
    public Doctor Doctor { get; set; }

    public ICollection<PrescriptionMedicament> PrescriptionMedicaments { get; set; }
}