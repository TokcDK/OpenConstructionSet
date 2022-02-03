﻿namespace OpenConstructionSet.Data;

/// <summary>
/// Represents an <see cref="Instance"/> from the game's data.
/// </summary>
public class Instance
{
    /// <summary>
    /// Creates a new <see cref="Instance"/> from another.
    /// </summary>
    /// <param name="instance">The <see cref="Instance"/> to copy.</param>
    public Instance(Instance instance)
    {
        Id = instance.Id;
        TargetId = instance.TargetId;
        Position = instance.Position;
        Rotation = instance.Rotation;
        States = new(instance.States);
    }

    /// <summary>
    /// Creates a new <see cref="Instance"/> from the provided data.
    /// </summary>
    /// <param name="id">The identifier for the <see cref="Instance"/>.</param>
    /// <param name="targetId">The <see cref="Item.StringId"/> of the targeted <see cref="Item"/>.</param>
    /// <param name="position">The <see cref="Instance"/>'s position.</param>
    /// <param name="rotation">The <see cref="Instance"/>'s rotation.</param>
    /// <param name="states">A collection of states for the <see cref="Instance"/>.</param>
    public Instance(string id, string targetId, Vector3 position, Vector4 rotation, IEnumerable<string> states)
    {
        this.Id = id;
        TargetId = targetId;
        Position = position;
        Rotation = rotation;
        States = new(states);
    }

    /// <summary>
    /// The <see cref="Instance"/>'s identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The <see cref="Instance"/>'s position.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// The <see cref="Instance"/>'s rotation.
    /// </summary>
    public Vector4 Rotation { get; set; }

    /// <summary>
    /// A collection of states for the <see cref="Instance"/>.
    /// </summary>
    public List<string> States { get; } = new();

    /// <summary>
    /// The <see cref="Item.StringId"/> of the targeted <see cref="Item"/>.
    /// </summary>
    public string TargetId { get; set; }

    /// <summary>
    /// Marks this <see cref="Instance"/> as deleted.
    /// </summary>
    public void Delete() => TargetId = string.Empty;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Instance instance &&
               Id == instance.Id &&
               Position.Equals(instance.Position) &&
               Rotation.Equals(instance.Rotation) &&
               TargetId == instance.TargetId &&
               States.SequenceEqual(instance.States);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var statesValue = new HashCode();

        States.ForEach(s => statesValue.Add(s));

        return HashCode.Combine(Id, Position, Rotation, TargetId, statesValue.ToHashCode());
    }

    /// <summary>
    /// Determines if this <see cref="Instance"/> is marked as deleted.
    /// </summary>
    /// <returns><c>true</c> if marked as deleted; otherwise, <c>false</c>.</returns>
    public bool IsDeleted() => TargetId.Length == 0;

    /// <inheritdoc/>
    public override string ToString() => $"{Id} Target={TargetId} Postion={Position} Rotation={Rotation} States={string.Join(", ", States)}";
}
