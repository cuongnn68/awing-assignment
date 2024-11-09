// Base Entity class with id column in guid, map column in jsonb, result colum in number

using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TreasureMap
{
    [Key] [DataType("uuid")] public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    [DataType("jsonb")] public string Map { get; set; } = "{}";

    public int Result { get; set; }
}