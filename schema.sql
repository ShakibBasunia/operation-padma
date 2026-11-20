-- Same table EF Core creates for MatchResult. Works on MySQL / MS SQL with minor type changes.
CREATE TABLE Matches (
    Id        VARCHAR(36) PRIMARY KEY,
    Player    VARCHAR(20) NOT NULL,
    Winner    VARCHAR(4)  NOT NULL,
    Rounds    INT NOT NULL,
    BlueLeft  INT NOT NULL,
    RedLeft   INT NOT NULL,
    PlayedAt  DATETIME NOT NULL
);

-- Leaderboard: fastest wins
SELECT Player, MIN(Rounds) AS BestRounds, COUNT(*) AS Wins
FROM Matches WHERE Winner = 'Blue'
GROUP BY Player ORDER BY BestRounds ASC, Wins DESC LIMIT 10;
