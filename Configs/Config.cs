namespace ProjectMER.Configs;

public class Config
{
	public List<string> OnWaitingForPlayers { get; set; } = new();
	public List<string> OnRoundStarted { get; set; } = new();
	public List<string> OnLczDecontaminationStarted { get; set; } = new();
	public List<string> OnWarheadStarted { get; set; } = new();
	public List<string> OnWarheadStopped { get; set; } = new();
	public List<string> OnWarheadDetonated { get; set; } = new();
}
