-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Gegenereerd op: 24 jun 2025 om 14:25
-- Serverversie: 10.11.9-MariaDB
-- PHP-versie: 8.2.28

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `pepijnluchtmeije`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `player_info`
--

CREATE TABLE `player_info` (
  `UserID` int(11) DEFAULT NULL,
  `TotalChips` int(11) DEFAULT NULL,
  `FreeChipCooldown` time DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

--
-- Gegevens worden geëxporteerd voor tabel `player_info`
--

INSERT INTO `player_info` (`UserID`, `TotalChips`, `FreeChipCooldown`) VALUES
(1, 1500, '01:24:20'),
(18, 1480, '01:24:24'),
(42, 1000, NULL);

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `poker_match`
--

CREATE TABLE `poker_match` (
  `GameID` int(11) DEFAULT NULL,
  `PlayerCount` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `poker_players`
--

CREATE TABLE `poker_players` (
  `UserID` int(11) DEFAULT NULL,
  `MatchID` int(11) DEFAULT NULL,
  `Waiting` tinyint(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `poker_scores`
--

CREATE TABLE `poker_scores` (
  `ScoreID` int(11) NOT NULL,
  `UserID` int(11) DEFAULT NULL,
  `Score` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

--
-- Gegevens worden geëxporteerd voor tabel `poker_scores`
--

INSERT INTO `poker_scores` (`ScoreID`, `UserID`, `Score`) VALUES
(1, 18, 1700),
(2, 18, 2200),
(3, 18, 2500),
(4, 18, 900),
(5, 18, 1200),
(9, 1, 400),
(11, 1, 300),
(19, 1, 40),
(24, 18, 100),
(25, 1, 140),
(26, 1, 100),
(27, 42, 300);

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `Scores`
--

CREATE TABLE `Scores` (
  `ID` int(11) NOT NULL,
  `Score` varchar(255) DEFAULT NULL,
  `UserID` int(11) DEFAULT NULL,
  `ScoredAt` timestamp NULL DEFAULT NULL,
  `GameID` int(11) DEFAULT NULL,
  `ServerID` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

--
-- Gegevens worden geëxporteerd voor tabel `Scores`
--

INSERT INTO `Scores` (`ID`, `Score`, `UserID`, `ScoredAt`, `GameID`, `ServerID`) VALUES
(42, '888', 1, '2025-05-28 21:08:36', 1, 1),
(43, '534', 1, '2025-05-28 21:20:52', 1, 1),
(44, '586', 1, '2025-05-28 21:43:15', 1, 1),
(45, '235', 1, '2025-05-28 21:43:19', 1, 1),
(46, '957', 1, '2025-05-28 21:43:25', 1, 1),
(47, '382', 1, '2025-05-28 21:43:29', 1, 1),
(48, '512', 2, '2025-05-28 22:14:57', 1, 1),
(49, '821', 2, '2025-05-28 22:15:01', 1, 1),
(50, '111', 2, '2025-05-28 22:15:05', 1, 1),
(51, '92', 2, '2025-05-28 22:15:09', 1, 1),
(53, '1245', 1, '2020-05-06 20:30:54', 1, 1),
(54, '235', 1, '2025-06-14 14:55:17', 1, 1),
(55, '9856', 1, '2025-06-14 14:56:55', 1, 1);

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `Servers`
--

CREATE TABLE `Servers` (
  `ID` int(11) DEFAULT NULL,
  `Pass` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

--
-- Gegevens worden geëxporteerd voor tabel `Servers`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `Users`
--

CREATE TABLE `Users` (
  `ID` int(11) NOT NULL,
  `Username` varchar(255) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Pass` varchar(255) DEFAULT NULL,
  `Country` varchar(255) DEFAULT NULL,
  `DateOfBirth` timestamp NULL DEFAULT '0000-00-00 00:00:00'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_520_ci;

--
-- Gegevens worden geëxporteerd voor tabel `Users`
--

--
-- Indexen voor geëxporteerde tabellen
--

--
-- Indexen voor tabel `poker_scores`
--
ALTER TABLE `poker_scores`
  ADD PRIMARY KEY (`ScoreID`);

--
-- Indexen voor tabel `Scores`
--
ALTER TABLE `Scores`
  ADD PRIMARY KEY (`ID`);

--
-- Indexen voor tabel `Users`
--
ALTER TABLE `Users`
  ADD PRIMARY KEY (`ID`),
  ADD UNIQUE KEY `Username` (`Username`,`Email`);

--
-- AUTO_INCREMENT voor geëxporteerde tabellen
--

--
-- AUTO_INCREMENT voor een tabel `poker_scores`
--
ALTER TABLE `poker_scores`
  MODIFY `ScoreID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=28;

--
-- AUTO_INCREMENT voor een tabel `Scores`
--
ALTER TABLE `Scores`
  MODIFY `ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=66;

--
-- AUTO_INCREMENT voor een tabel `Users`
--
ALTER TABLE `Users`
  MODIFY `ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=43;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
