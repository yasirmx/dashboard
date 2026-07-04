namespace Tasks.Application.Models;

public record BoardColumnDto(string Status, IReadOnlyList<TaskDto> Tasks);

public record BoardDto(Guid SprintId, IReadOnlyList<BoardColumnDto> Columns);
