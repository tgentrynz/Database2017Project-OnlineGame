/*
Description:
	Run this script to create the database
*/
-- Set the delimiter for the script
DELIMITER //
/*SECTION 1: DATABASE RECREATION*/
DROP SCHEMA IF EXISTS db_gamedb//
CREATE SCHEMA db_gamedb//
USE db_gamedb//
/*SECTION 2: TABLE CREATION PROCEDURE*/
CREATE PROCEDURE proc_initializeTables()
BEGIN
	SET foreign_key_checks = 0; -- Stop foreign key checks so the tables can be deleted.
	
	-- Delete any old table instances
	DROP TABLE IF EXISTS tbl_player;
	DROP TABLE IF EXISTS tbl_game;
	DROP TABLE IF EXISTS tbl_actions;
	DROP TABLE IF EXISTS tbl_player_game;
	DROP TABLE IF EXISTS tbl_player_actions;
	
	-- Create new tables
	CREATE TABLE tbl_player
	(
		id INT PRIMARY KEY AUTO_INCREMENT,
		uname VARCHAR(16) NOT NULL UNIQUE,
		pword VARCHAR(16) NOT NULL,
		currentWinStreak SMALLINT NOT NULL DEFAULT 0,
		highestWinStreak SMALLINT NOT NULL DEFAULT 0,
		failedLoginAttempts TINYINT NOT NULL DEFAULT 0,
		lastActivityTime DATETIME NOT NULL DEFAULT NOW(),
		isOnline BOOLEAN NOT NULL DEFAULT FALSE,
		isLocked BOOLEAN NOT NULL DEFAULT FALSE,
		isAdmin BOOLEAN NOT NULL DEFAULT FALSE
	);

	CREATE TABLE tbl_game
	(
		id INT PRIMARY KEY AUTO_INCREMENT,
		gameState TINYINT NOT NULL DEFAULT 0,
		resetNumber INT NOT NULL DEFAULT 0,
		roundNumber SMALLINT,
		stateStartTime DATETIME NOT NULL DEFAULT NOW(),
		gameStartTime DATETIME NOT NULL DEFAULT NOW(),
		gameEndTime DATETIME
	);

	CREATE TABLE tbl_actions
	(
		id TINYINT PRIMARY KEY,
		weakTo TINYINT UNIQUE NOT NULL,
		-- Create foreign key references
		CONSTRAINT fk_weakTo
			FOREIGN KEY(weakTo)
			REFERENCES tbl_actions(id)
			ON DELETE RESTRICT
	);
	INSERT INTO tbl_actions(id, weakTo)
		VALUES
			(0,1),
			(1,2),
			(2,0);

	CREATE TABLE tbl_player_game
	(
		id INT PRIMARY KEY AUTO_INCREMENT,
		pID INT NOT NULL,
		gID INT NOT NULL,
		isHost BOOLEAN NOT NULL DEFAULT FALSE,
		playerState TINYINT NOT NULL DEFAULT 0,
		joinTime DATETIME NOT NULL DEFAULT NOW(),
		-- Create composite primary key using pID and gID
		CONSTRAINT uk_compositeKeyPlayerAndGame
			UNIQUE KEY(pID, gID),
		-- Create foreign key references
		CONSTRAINT fk_playerID
			FOREIGN KEY(pID)
			REFERENCES tbl_player(id),
		CONSTRAINT fk_gameID 
			FOREIGN KEY(gID)
			REFERENCES tbl_game(id)
			ON DELETE CASCADE
	);

	CREATE TABLE tbl_player_actions
	(
		playerInstanceID INT NOT NULL,
		resetNumber INT NOT NULL,
		roundNumber SMALLINT NOT NULL,
		playerAction TINYINT,
		-- Create composite primary key using playerInstanceID, resetNumber and roundNumber
		CONSTRAINT pk_compositeKeyInstResetRound
			PRIMARY KEY(playerInstanceID, resetNumber, roundNumber),
		-- Create foreign key references
		CONSTRAINT fk_playerInstanceID
			FOREIGN KEY(playerInstanceID)
			REFERENCES tbl_player_game(id)
			ON DELETE CASCADE,
		CONSTRAINT fk_action
			FOREIGN KEY(playerAction)
			REFERENCES tbl_actions(id)
			ON DELETE SET NULL
	);
	
	SET foreign_key_checks = 1; -- Start foreign key checks again.
END//
CALL proc_initializeTables()//

/*SECTION 3: ROUTINE CREATION*/
CREATE FUNCTION func_playerIsInactive(prm_playerID INT) RETURNS BOOLEAN
BEGIN
	/*
	Description:
		A function that checks a player account for inactivity
	Input:
		The id of the player
	Output:
		If the account is inactive
		Boolean value
	*/
	RETURN TIME_TO_SEC(
			TIMEDIFF(
				NOW(),
				(
					SELECT lastActivityTime
						FROM tbl_player
						WHERE id = prm_playerID
				)
			)
		) > 599;
END //

CREATE FUNCTION func_getHostName(prm_gameID INT) RETURNS VARCHAR(16)
BEGIN
	/*
	Description
		Function to return the username value for the host of a game
	Input:
		The id of the game
	Output:
		The username of the host
		String value
	*/
	RETURN
		( 
			SELECT P.uName
				FROM tbl_player AS P
				INNER JOIN
					(
						tbl_player_game AS PG
							INNER JOIN tbl_game AS G
								ON PG.gID = G.id
					)
					ON P.id = PG.pID
				WHERE G.id = prm_gameID
					AND PG.isHost = TRUE
		);
END//

CREATE FUNCTION func_getPlayerCount(prm_gameID INT) RETURNS INT
BEGIN
	/*
	Description:
		Function to get the number of active players in a game.
	Input:
		The id of the game
	Output:
		The number of players
		Integer value
	*/
	RETURN
		(
			SELECT COUNT(pID)
				FROM tbl_player_game
				WHERE gID = prm_gameID
					AND playerState != 2
		);
END//

CREATE FUNCTION func_newPlayerInstance(prm_playerID INT, prm_gameID INT) RETURNS INT
BEGIN
	/*
	Description:
		Function to add a player to an existing game
	Insert:
		The id of the player
		The id of the game
	Output:
		The id of the new player instance
		Integer value
	*/
	-- Output variable declaration
	DECLARE loc_outvar INT DEFAULT NULL;
	IF (SELECT COUNT(id) FROM tbl_player_game WHERE pID = prm_playerID AND gID = prm_gameID) > 0 THEN
		-- If the player is rejoining a game
		UPDATE tbl_player_game
			SET playerState = 0, joinTime = DEFAULT
			WHERE pID = prm_playerID
				AND gID = prm_gameID;
		SET loc_outvar = (SELECT id FROM tbl_player_game WHERE pID = prm_playerID AND gID = prm_gameID);
	ELSE 
		-- If the player is new to this game
		INSERT INTO tbl_player_game (pID, gID)
			VALUES (prm_playerID, prm_gameID);
		SET loc_outvar = LAST_INSERT_ID();
	END IF;
	-- Return value;
	RETURN loc_outvar;
END //

CREATE FUNCTION func_actionCount(prm_gameID INT, prm_reset INT, prm_round SMALLINT) RETURNS TINYINT
BEGIN
	/*
	Description:
		Function to return the number of different actions played in a round.
	Input:
		An id for the game to retrieve values for
		A number for the reset iteration to retrieve values for
		A number for the round to retrieve values for
	Output:
		The number of distinct playerAction values present
		Integer value
	*/
	-- Output variable declaration
	DECLARE loc_outvar TINYINT DEFAULT 0;
	-- Select query to get the count of distinct actions present
	SELECT COUNT(DISTINCT playerAction) INTO loc_outvar
		FROM tbl_player_actions AS PA
		INNER JOIN tbl_player_game AS PG
		ON PA.playerInstanceId = PG.id
		WHERE PG.gID = prm_gameID
			AND PA.resetNumber = prm_reset
			AND PA.roundNumber = prm_round;
	-- Return value
	RETURN loc_outvar;
END//

CREATE FUNCTION func_actionCountHighest(prm_gameID INT, prm_reset INT, prm_round SMALLINT) RETURNS TINYINT
BEGIN
	/*
	Description:
		Function to return the number of actions in a round to get the highest number of votes. Used to determine ties.
	Input:
		An id for the game to retrieve values for
		A number for the reset iteration to retrieve values for
		A number for the round to retrieve values for
	Output:
		The number of distinct playerAction values present that had a play count equal the highest play count
		Integer value
	*/
	-- Output variable declaration
	DECLARE loc_outvar TINYINT DEFAULT 0;
	-- Select query that returns the count of distinct actions played that had a play count equal to the highest
    SELECT COUNT(countPlayerAction) INTO loc_outvar
		FROM
			(
				-- Select subquery that returns the count of each distinct action based on the input game id, reset# and round#
				SELECT COUNT(playerAction) AS countPlayerAction
					FROM tbl_player_actions AS PA
					INNER JOIN tbl_player_game AS PG
					ON PA.playerInstanceID = PG.id
					WHERE PG.gID = prm_gameID
						AND PA.resetNumber = prm_reset
						AND PA.roundNumber = prm_round
					GROUP BY playerAction
			) AS derv -- Derived table alias
		WHERE countPlayerAction = 
			(
				-- Select subquery to get the highest play count
				SELECT COUNT(playerAction) AS countPlayerAction
					FROM tbl_player_actions AS PA
					INNER JOIN tbl_player_game AS PG
					ON PA.playerInstanceID = PG.id
					WHERE PG.gID = prm_gameID
						AND PA.resetNumber = prm_reset
						AND PA.roundNumber = prm_round
					GROUP BY playerAction
					ORDER BY countPlayerAction DESC
					LIMIT 1
			);
	RETURN loc_outvar;
END//

CREATE FUNCTION func_calculateLosingAction(prm_gameID INT, prm_reset INT, prm_round SMALLINT) RETURNS TINYINT
BEGIN
	/*
	Description:
		Function that returns the id of the losing action for a given game-reset-round.
	Input:
		An id for the game to retrieve values for
		A number for the reset iteration to retrieve values for
		A number for the round to retrieve values for
	Output:
		The id of the losing action
		Integer value
		Null returns expected
	*/
	-- Output variable declaration
	DECLARE loc_outvar TINYINT DEFAULT NULL;
	-- Declaration of variables to store action ids to pass to other routines
	DECLARE loc_actionOne TINYINT DEFAULT NULL;
	DECLARE loc_actionTwo TINYINT DEFAULT NULL; 
	-- Case query on the result of func_actionCount
	CASE func_actionCount(prm_gameID, prm_reset, prm_round)
		WHEN 1 THEN
			-- One action played
			-- This means no one has lost, so return null
			SET loc_outvar = NULL;
		WHEN 2 THEN
			-- Two actions played
			-- This means the loser is decided by standard RPS rules
			SET loc_actionOne =
			(
				-- Select query to get the id of the first action played
				SELECT playerAction
					FROM tbl_player_actions AS PA
					INNER JOIN tbl_player_game AS PG
						ON PA.playerInstanceId = PG.id
					WHERE PG.gID = prm_gameID
						AND PA.resetNumber = prm_reset
						AND PA.roundNumber = prm_round
					GROUP BY playerAction
					LIMIT 1
			);
			SET loc_actionTwo =
			(
				-- Select quety to get the id of the action that isn't the first one
				SELECT playerAction
					FROM tbl_player_actions AS PA
					INNER JOIN tbl_player_game AS PG
						ON PA.playerInstanceId = PG.id
					WHERE PG.gID = prm_gameID
						AND PA.resetNumber = prm_reset
						AND PA.roundNumber = prm_round
					GROUP BY playerAction
					HAVING playerAction != loc_actionOne -- Was DISTINCT playerAction, but that didn't work
			);
			-- Compare the actions and store to value for return
			SET loc_outvar = func_compareActions(loc_actionOne, loc_actionTwo);
		WHEN 3 THEN
			-- Three actions played
			-- This means the loser is decided by the advanced play count rules
			CASE func_actionCountHighest(prm_gameID, prm_reset, prm_round)
				WHEN 1 THEN
					-- One action won
					-- This means the loser is the action weak to this one
					SET loc_outvar =
					(
						-- Select query to return the id column from the rown in actions where the weakTo is the winning action
						SELECT id
							FROM tbl_actions
							WHERE weakTo =
							(
								-- Select subquery to return the id of the winning action
								SELECT playerAction
									FROM tbl_player_actions AS PA
									INNER JOIN tbl_player_game AS PG
										ON PA.playerInstanceId = PG.id
									WHERE PG.gID = prm_gameID
										AND PA.resetNumber = prm_reset
										AND PA.roundNumber = prm_round
									GROUP BY playerAction
									HAVING COUNT(playerAction) = func_playCountHighest(prm_gameID, prm_reset, prm_round)
							)
					);
				WHEN 2 THEN
					-- Two actions won
					-- This means the loser is decided based on standard RPS rules using these two actions
					SET loc_actionOne =
					(
						-- Select query to get the id of the first action that won.
						SELECT playerAction
							FROM tbl_player_actions AS PA
							INNER JOIN tbl_player_game AS PG
								ON PA.playerInstanceId = PG.id
							WHERE PG.gID = prm_gameID
								AND PA.resetNumber = prm_reset
								AND PA.roundNumber = prm_round
							GROUP BY playerAction
							HAVING COUNT(playerAction) = func_playCountHighest(prm_gameID, prm_reset, prm_round)
							LIMIT 1
					);
					-- Select query to get the id of the action that won and wasn't the first one selected.
					SET loc_actionTwo =
					(
						SELECT playerAction
							FROM tbl_player_actions AS PA
							INNER JOIN tbl_player_game AS PG
								ON PA.playerInstanceId = PG.id
							WHERE PG.gID = prm_gameID
								AND PA.resetNumber = prm_reset
								AND PA.roundNumber = prm_round
							GROUP BY playerAction
							HAVING COUNT(playerAction) = func_playCountHighest(prm_gameID, prm_reset, prm_round)
								AND playerAction != loc_actionOne
					);
					-- Compare the actions and store to value for return
					SET loc_outvar = func_compareActions(loc_actionOne, loc_actionTwo);
				WHEN 3 THEN
					-- Three actions won
					-- This means no one has lost, so return null
					SET loc_outvar = NULL;
				ELSE
					-- This case shouldn't run, return null just in case
					SET loc_outvar = NULL;
			END CASE; 
		ELSE
			-- This means no one played, return null
			SET loc_outvar = NULL;
	END CASE;
	RETURN loc_outvar;
END//

CREATE FUNCTION func_compareActions(prm_a TINYINT, prm_b TINYINT) RETURNS TINYINT
BEGIN
	/*
	Description:
		Function that returns the weak action of two input actions
	Input:
		First action id
		Second action id
	Output:
		The id of the weak action
		Integer value
	*/
	-- Output variable declaration
	DECLARE loc_outvar TINYINT DEFAULT NULL;
	-- Check the two actions are different
	IF prm_a != prm_b THEN
		IF ((SELECT weakTo FROM tbl_actions WHERE id = prm_a) = prm_b) THEN -- If input a is the weakest action
			SET loc_outvar = prm_a;
		ELSE -- If input b is the weakest action
			SET loc_outvar = prm_b;
		END IF;
	END IF;
	-- Return value
	RETURN loc_outvar;
END //

CREATE FUNCTION func_currentResetNumber(prm_gameID INT) RETURNS INT
BEGIN
	/*
	Description:
		Function that returns the current reset iteration number for a game
	Input:
		Game id
	Output:
		The resetNumber value of the game
		Integer value
	*/
	RETURN
		(
			-- Select query that gets the resetNumber value
			SELECT resetNumber
				FROM tbl_game
				WHERE id = prm_gameID
		);
END //

CREATE FUNCTION func_currentRoundNumber(prm_gameID INT) RETURNS SMALLINT
BEGIN
	/*
	Description:
		Function that returns the current round number for a game
	Input:
		Game id
	Output:
		The roundNumber value of the game
		Integer value
	*/
	RETURN
		(
			-- Select query that gets the roundNumber value
			SELECT roundNumber
				FROM tbl_game
				WHERE id = prm_gameID
		);
END //

CREATE FUNCTION func_gameUpdateReady(prm_gameID INT) RETURNS BOOLEAN
BEGIN
	/*
	Description:
		Function to determine if a game entity needs it's state updated based on a timer
	Input:
		The id of the game being checked
	Output:
		Whether the game state should be updated
		Boolean value
	*/
	-- Output variable declaration
	DECLARE loc_outvar BOOLEAN DEFAULT FALSE;
	-- Local variable declarations
	DECLARE loc_timeComp INT DEFAULT TIME_TO_SEC( -- Variable to store how long the state has lasted for
			TIMEDIFF(
				NOW(),
				(
					SELECT stateStartTime
						FROM tbl_game
						WHERE id = prm_gameID
				)
			)
		);
	CASE func_getGameState(prm_gameID)
		WHEN 0 THEN -- Waiting state
			SET loc_outvar = TRUE;
		WHEN 1 THEN -- Starting state
			SET loc_outvar = TRUE;
		WHEN 2 THEN -- Selection state
			SET loc_outvar = (loc_timeComp > 29); -- Update if state has lasted at least 30 seconds
		WHEN 3 THEN -- Checking state
			SET loc_outvar = TRUE;
		WHEN 4 THEN -- Results state
			SET loc_outvar = (loc_timeComp > 9); -- Update if state has lasted at least 10 seconds
		WHEN 5 THEN -- Over state
			SET loc_outvar = (loc_timeComp > 9); -- Update if state has lasted at least 10 seconds
		WHEN 6 THEN -- Ended state
			SET loc_outvar = FALSE;
		ELSE -- Only runs in the case of unexpected behaviour
			BEGIN
			END;
	END CASE;
	-- Return value
	RETURN loc_outvar;
END//

CREATE FUNCTION func_getGameState(prm_gameID INT) RETURNS TINYINT
BEGIN
	/*
	Description:
		Function to get the state of a game
	Input:
		The id of the game
	Output:
		The state of the game
	*/
	RETURN
		(
			SELECT gameState
				FROM tbl_game
				WHERE id = prm_gameID
		);
END//

CREATE FUNCTION func_playCountHighest(prm_gameID INT, prm_reset INT, prm_round SMALLINT) RETURNS INT
BEGIN
	/*
	Description:
		Function to return the highest play count value
	Input:
		An id for the game to retrieve values for
		A number for the reset iteration to retrieve values for
		A number for the round to retrieve values for
	Output:
		The highest play count
		Integer value
	*/
	-- Output variable declaration
	DECLARE loc_outvar INTEGER DEFAULT 0;
	-- Select query to return the highest play count
	SET loc_outvar =
		(
			SELECT COUNT(playerAction) AS c
				FROM tbl_player_actions AS PA
				INNER JOIN tbl_player_game AS PG
						ON PA.playerInstanceId = PG.id
				WHERE PG.gID = prm_gameID
					AND PA.resetNumber = prm_reset
					AND PA.roundNumber = prm_round
			GROUP BY playerAction
			ORDER BY c DESC
			LIMIT 1
		);
	-- Return value
	RETURN loc_outvar;
END//

CREATE FUNCTION func_comparePassword(prm_id INT, prm_pWord VARCHAR(16)) RETURNS BOOLEAN
BEGIN
	/*
	Description:
		Function to compare passwords
	Input:
		The id of the account
		The string to compare
	Output:
		Result of password comparison
		Boolean value
	*/
	RETURN
		(
			prm_pWord LIKE
			(
				SELECT pWord
					FROM tbl_player
					WHERE id = prm_id
			)
		);
END//

CREATE PROCEDURE proc_playerLogout(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to handle a player logging out
	Input:
		The player id
	Output:
		-
	*/
	DECLARE n INT DEFAULT 0;
	DECLARE i INT DEFAULT 0;
	DECLARE idToUpdate INT DEFAULT NULL;
	-- Get number of rows to edit
	SELECT COUNT(id) INTO n
		FROM tbl_player_game
		WHERE pID = prm_playerID;
	-- Reset iterator
	SET i = 0;
	WHILE i < n DO
		-- Get instance id of player
		SET idToUpdate =
			(
				SELECT id
					FROM tbl_player_game
					WHERE pID = prm_playerID
					LIMIT 1 OFFSET i
			);
		-- Remove the instance from its game
		CALL proc_playerDropout(idToUpdate);
		-- Update the iterator
		SET i = i + 1;
	END WHILE;
	-- Set player to offline
	UPDATE tbl_player
		SET isOnline = FALSE
		WHERE id = prm_playerID;
END//

CREATE PROCEDURE proc_updatePlayerActivityTime(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to update the player last activity time to the current time
	Input:
		The player id
	Output:
		-
	*/
	UPDATE tbl_player
		SET lastActivityTime = NOW()
		WHERE id = prm_playerID;
END//

CREATE PROCEDURE proc_endGame(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to end a game for good
	Input:
		The id of the game
	Output:
		-
	*/
	UPDATE tbl_game
		SET
			gameEndTime = NOW(),
			gameState = 6
		WHERE id = prm_gameID;
	UPDATE tbl_player_game
		SET playerState = 2
		WHERE gID = prm_gameID;
END//

CREATE PROCEDURE proc_playerDropout(IN prm_instanceID INT)
BEGIN
	/*
	Description:
		Procedure to handle the removal of a player from a game.
	Input:
		The player instance to remove
	Output:
		-
	*/
	-- Local variable declaration
	DECLARE loc_gameID INT DEFAULT
		(
			-- Select subquery that gets the game ID
			SELECT gID
				FROM tbl_player_game
				WHERE id = prm_instanceID
		);
	IF ((SELECT isHost FROM tbl_player_game WHERE id = prm_instanceID) = TRUE) THEN
		-- If player was flagged as host then find a new host
		CALL proc_updateGameHost(loc_gameID, prm_instanceID);
	END IF;
	-- If the game will be empty after this player leaves, end it
	IF (func_getPlayerCount(loc_gameID) = 1) THEN
		CALL proc_endGame(loc_gameID);
	END IF;
	-- Remove the player from the game
	UPDATE tbl_player_game
		SET playerState = 2
		WHERE id = prm_instanceID;
END//

CREATE PROCEDURE proc_updateGameHost(prm_gameID INT, prm_currentHostID INT)
BEGIN
	/*
	Description:
		Procedure to change the host of the game to the next candidiate
	Input:
		The id of the game
		The id of the current host
	Output:
		-
	*/
	-- Local variable declaration
	DECLARE loc_newHost INT DEFAULT NULL;
	-- remove host flag from current host 
	UPDATE tbl_player_game
		SET isHost = FALSE
		WHERE id = prm_currentHostID;
	-- find a new host
	SET loc_newHost =
			(
				-- Select query that returns first candidate for host
				SELECT derv.temp_id
					FROM
						(
							SELECT iDerv.iID AS temp_id, iDerv.iTD AS temp_time
								FROM
									(
										-- Select subquery that gets a list of players
										SELECT iPG.id AS iID, TIME_TO_SEC(TIMEDIFF(iPG.joinTime, iG.gameStartTime)) AS iTD
											FROM tbl_player_game AS iPG
											INNER JOIN tbl_game AS iG
												ON iPG.gID = iG.id
											WHERE iPG.gID = prm_gameID

									) AS iDerv -- Derived table alias
								HAVING temp_time = 
									(
										-- Select subquery to get the smallest time difference
										SELECT TIME_TO_SEC(TIMEDIFF(iPG.joinTime, iG.gameStartTime)) AS iTD
											FROM tbl_player_game AS iPG
											INNER JOIN tbl_game AS iG
												ON iPG.gID = iG.id
											WHERE iPG.gID = prm_gameID
												AND iPG.id != prm_currentHostID
											GROUP BY iTD
											ORDER BY iTD ASC
											LIMIT 1
									)
						) AS derv -- Derived table alias
					WHERE derv.temp_id != prm_currentHostID
						AND 
							(
								SELECT playerState
									FROM tbl_player_game
									WHERE id = derv.temp_id
							) != 2
					LIMIT 1
			);
	-- and flag that new host
	UPDATE tbl_player_game
		SET isHost = TRUE
		WHERE id = loc_newHost;
END//

CREATE PROCEDURE proc_updateGameState(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure that updates a game's state
	Input:
		The game id
	Output:
		-
	*/
	-- Local variable declaration
	DECLARE loc_newState TINYINT DEFAULT 6;
	DECLARE loc_losingAction TINYINT DEFAULT NULL; -- Variable to store the losing action in the selection state
	DECLARE loc_resetNumber INT DEFAULT func_currentResetNumber(prm_gameID);
	DECLARE loc_roundNumber SMALLINT DEFAULT func_currentRoundNumber(prm_gameID);
	DECLARE loc_playerCount INT DEFAULT func_getPlayerCount(prm_gameID);
	
	
	IF(loc_playerCount = 0) THEN -- Check if all players have left the game.
		SET loc_newState = 6; -- Force end the game.
	ELSEIF (loc_playerCount = 1)THEN -- Check if only one player remains
		SET loc_newState = 0; -- Force the game to return to waiting
	ELSE -- Handle the update of game
		CASE (SELECT gameState FROM tbl_game WHERE id = prm_gameID)
			WHEN 0 THEN -- Waiting state
				IF ( (SELECT COUNT(id) FROM tbl_player_game WHERE gID = prm_gameID) > 1) THEN
					SET loc_newState = 1; -- Move to starting state if there is at least 2 players
				ELSE
					SET loc_newState = 0; -- State remains the same
				END IF;
			WHEN 1 THEN -- Starting state
				-- Set the round number to 1
				UPDATE tbl_game
					SET roundNumber = 1
					WHERE id = prm_gameID;
				-- Set the state of all waiting players to active
				UPDATE tbl_player_game
					SET playerState = 1
					WHERE gID = prm_gameID
						AND playerState = 0;
				SET loc_newState = 2; -- Go to selection state
			WHEN 2 THEN -- Selection state
				-- Set the state of all idle players to waiting
				UPDATE tbl_player_game
					SET playerState = 0
					WHERE gID = prm_gameID
						AND NOT EXISTS
							(
								SELECT playerInstanceID
									FROM tbl_player_actions
									WHERE playerInstanceID = tbl_player_game.id
										AND resetNumber = loc_resetNumber
										AND roundNumber = loc_roundNumber
							);
				SET loc_newState = 3; -- Go to checking state
			WHEN 3 THEN -- Checking state
				-- Update the state of all losing players to waiting
				SET loc_losingAction = func_calculateLosingAction(prm_gameID, loc_resetNumber, loc_roundNumber);
				IF(loc_losingAction IS NOT NULL) THEN
					UPDATE tbl_player_game AS PG
					INNER JOIN tbl_player_actions AS PA
						ON PG.id = PA.playerInstanceID
					SET PG.playerState = 0
					WHERE PG.gID = prm_gameID
						AND PA.resetNumber = loc_resetNumber
						AND PA.roundNumber = loc_roundNumber
						AND PA.playerAction = loc_losingAction;
				END IF;
				SET loc_newState = 4; -- Go to result state
			WHEN 4 THEN -- Results state
				IF ((SELECT COUNT(id) FROM tbl_player_game WHERE gID = prm_gameID AND playerState = 1) < 2) THEN
					-- If there is 1 or 0 players left active
					CALL proc_updateWinStreak(prm_gameID);
					SET loc_newState = 5; -- Go to over state
				ELSE
					-- If there are still enough players to playerAction
					-- Update round numbers
					UPDATE tbl_game
						SET roundNumber = roundNumber+1
						WHERE id = prm_gameID;
					SET loc_newState = 2; -- Go to selection state
				END IF;
			WHEN 5 THEN -- Over state
				-- Update the reset and round numbers
				UPDATE tbl_game
					SET resetNumber = resetNumber+1, roundNumber = NULL
					WHERE id = prm_gameID;
				-- Reset player states
				UPDATE tbl_player_game
					SET playerState = 0
					WHERE gID = prm_gameID
						AND playerState = 1;
				SET loc_newState = 1; -- Go to starting state
			WHEN 6 THEN -- Ended state
				SET loc_newState = 6; -- Can't change ended state
			ELSE -- Only runs in the case of unexpected behaviour
				BEGIN
				END;
		END CASE;
	END IF;
	-- Update the game's state value
	UPDATE tbl_game
		SET
			gameState = loc_newState,
			stateStartTime = NOW()
		WHERE id = prm_gameID;
END//

CREATE PROCEDURE proc_updateWinStreak(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure that updates the win streak information of players in a game
	Input:
		The id of the game
	Output:
		-
	*/
	-- Update the currentWinStreak of the winner
	UPDATE tbl_player AS P
		INNER JOIN tbl_player_game AS PG
			ON P.id = PG.pID
		SET P.currentWinStreak = P.currentWinStreak+1
		WHERE PG.gID = prm_gameID
			AND playerState = 1;
	-- Update the highestWinStreak if needed
	UPDATE tbl_player
		SET highestWinStreak = currentWinStreak
		WHERE currentWinStreak > highestWinStreak;
	-- Reset the currentWinStreak of losing players
	UPDATE tbl_player AS P
		INNER JOIN tbl_player_game AS PG
			ON P.id = PG.pID
		SET P.currentWinStreak = 0
		WHERE PG.gID = prm_gameID
			AND playerState = 0;
END//


CREATE PROCEDURE proc_updateLoginAttempts(IN prm_id INT)
BEGIN
	/*
	Description:
		Procedure that updates an account's failed login attempt value
	Input:
		Account id
	Output:
		-
	*/
	-- Store new attempt value for use
	DECLARE loc_updateVal TINYINT DEFAULT
		(
			SELECT failedLoginAttempts
				FROM tbl_player
				WHERE id = prm_id
		)+1;
	-- Update the failed attempts value
	UPDATE tbl_player
		SET failedLoginAttempts = loc_updateVal
		WHERE id = prm_id;
	-- Lock account if 5 failed attempts
	IF (loc_updateVal >= 5) THEN
		UPDATE tbl_player
			SET isLocked = TRUE
			WHERE id = prm_id;
	END IF;
END //

CREATE PROCEDURE intf_deletePlayer(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to allow a client to delete the existence of a player
	Input:
		The account id
	Output:
		The success of the action
		Boolean
	*/
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		SELECT FALSE AS `result`;
	END;
	-- Remove player from active games
	CALL proc_playerLogout(prm_playerID);
	-- Disable referential integrity
	SET foreign_key_checks = 0;
	-- Delete the player
	DELETE FROM tbl_player
		WHERE id = prm_playerID;
	-- Re-enable referential integrity
	SET foreign_key_checks = 1;
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_getPlayerInfo(IN prm_playerID INT)
BEGIN
	/*
	Description:
		A procedure that allows a client to get all the data for a single user
	Input:
		The id of the player to get data for
	Output:
		A row from the player table
	*/
	SELECT * FROM tbl_player
		WHERE id = prm_playerID;
END//

CREATE PROCEDURE intf_getPlayerList()
BEGIN
	/*
	Description:
		A procedure that allows a client to retrieve a list of all players
	Input:
		-
	Output:
		Rows from the player table
	*/
	SELECT * FROM tbl_player;
END//

CREATE PROCEDURE intf_isPlayerAdmin(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to allow a client to check if a player has administrator privileges
	Input:
		The account id
	Output:
		If the account is online
		Boolean value
	*/
	SELECT isAdmin AS `result`
		FROM tbl_player
		WHERE id = prm_playerID;
END//

CREATE PROCEDURE intf_playerLogout(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to allow a player to log out from the client
	Input:
		The player id
	Output:
		The success of the logout
		Boolean value
	*/
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		SELECT FALSE AS `result`;
	END;
	-- Remove player from active games
	CALL proc_playerLogout(prm_playerID);
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_updatePlayer(	IN prm_playerID INT,
									IN prm_uName VARCHAR(16),
									IN prm_pWord VARCHAR(16),
									IN prm_currentWinStreak SMALLINT,
									IN prm_highestWinStreak SMALLINT,
									IN prm_isLocked BOOLEAN,
									IN prm_isAdmin BOOLEAN
								)
BEGIN
	/*
	Description:
		Procedure that allows a client to update a row on the player table using parameters
	Input:
		The player id
		The new player username
		The new player password
		The new player win streak
		The new player highest win streak
		The new account locked state
		The new account admin state
	Output:
		The success of the action
		Boolean value
	*/
	-- Error handling declaration
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		SELECT FALSE AS `result`;
	END;
	
	UPDATE tbl_player
		SET uName = prm_uName,
			pWord = prm_pWord,
			currentWinStreak = prm_currentWinStreak,
			highestWinStreak = GREATEST(prm_currentWinStreak, prm_highestWinStreak),
			isLocked = prm_isLocked,
			isAdmin = prm_isAdmin
		WHERE id = prm_playerID;
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_updatePlayerActivityTime(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to allow a client to update a player's last activity time to the current time
	Input:
		The player id
	Output:
		The success of the update
		Boolean value
	*/
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		SELECT FALSE AS `result`; 
	END;
	-- Update
	UPDATE tbl_player
		SET lastActivityTime = NOW()
		WHERE id = prm_playerID;
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_getGameInfo(IN prm_gameID INT)
BEGIN
	/*
	Description
		Procedure to return information about a game to a client
	Input:
		The id of the game
	Output:
		The username of the host
		String value
		The number of player
		Integer value
	*/
	SELECT
		func_getHostName(prm_gameID) AS `host_name`,
		func_getPlayerCount(prm_gameID) AS `player_count`;
END//

CREATE PROCEDURE intf_getInstanceInfo(IN prm_instanceID INT)
BEGIN
	/*
	Description
		Procedure to return the game id of the a player instance for use in a client.
	Input:
		The id of the player instance.
	Output:
		The game id attached to the player instance
	*/
	SELECT * FROM tbl_player_game WHERE id = prm_instanceID;
END//

CREATE PROCEDURE intf_getInstanceSelection(IN prm_instanceID INT)
BEGIN
	/*
	Description
		Procedure to return the action a player selected in the current round for use in a client.
	Input:
		The id of the player instance.
	Output:
		The id of the action the user selected
		Null returns expected
	*/
	-- Local variable declaration
	DECLARE loc_gameID INT DEFAULT
		(
			SELECT gID
				FROM tbl_player_game
				WHERE id = prm_instanceID
		);
	SELECT playerAction
		FROM tbl_player_actions
		WHERE playerInstanceID = prm_instanceID
			AND resetNumber = func_currentResetNumber(loc_gameID)
			AND roundNumber = func_currentRoundNumber(loc_gameID);
END//

CREATE PROCEDURE intf_getRunningGames()
BEGIN
	/*
	Description:
		Procedure to get currently active games for a client
	Input:
		-
	Output:
		Rows from the game table
	*/
	SELECT * FROM tbl_game WHERE gameState != 6;
END//

CREATE PROCEDURE intf_initiateGame(IN prm_playerID INT)
BEGIN
	/*
	Description:
		Procedure to allow creation of new games from a client
	Input:
		Host id
	Output:
		The new game ID
		Integer value
		The ID of the new host instance
		Integer value
	*/
	-- Local variable declaration
	DECLARE loc_gameID INT;
	DECLARE loc_instanceID INT;
	-- Error handling declaration
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		SELECT
			NULL AS game_id,
			NULL AS instance_id;
	END;
	
	START TRANSACTION;
	-- Insert new game row
	INSERT INTO tbl_game ()
		VALUES ();
	-- Get the id of the newly created row
	SET loc_gameID = LAST_INSERT_ID();
	-- Add the player to the game
	SET loc_instanceID = func_newPlayerInstance(prm_playerID, loc_gameID);
	-- Make the player the host
	UPDATE tbl_player_game
		SET isHost = TRUE
		WHERE pID = prm_playerID
			AND gID = loc_gameID;
	COMMIT;
	SELECT
		loc_gameID AS game_id,
		loc_instanceID AS instance_id;
END//

CREATE PROCEDURE intf_newPlayerInstance(IN prm_playerID INT, IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to allow a client to enter a player into a game room
	Insert:
		The id of the game
	Output:
		The id of the new player instance
		Integer value
	*/
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		SELECT NULL AS `instance_id`;
	END;
	SELECT func_newPlayerInstance(prm_playerID, prm_gameID) AS `instance_id`;
END //

CREATE PROCEDURE intf_endGame(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to allow a game to be ended from the client
	Input:
		The id of the game
	Output:
		The success of the action
		Boolean value
	*/
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		SELECT FALSE AS `result`;
	END;
	CALL proc_endGame(prm_gameID);
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_getCountPerAction(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to allow a client to retrieve many players voted for each action in a given game-reset-round
	Input:
		An id for the game to retrieve values for

	Output:
		The action ID
		Integer value
		The count of playerState
		Integer value
	*/
	DECLARE loc_reset INT DEFAULT func_currentResetNumber(prm_gameID);
	DECLARE loc_round SMALLINT DEFAULT func_currentRoundNumber(prm_gameID);
	-- Select query to get the count of distinct actions present
	SELECT playerAction AS `Action`, COUNT(playerAction) AS `Count`
		FROM tbl_player_actions AS PA
		INNER JOIN tbl_player_game AS PG
		ON PA.playerInstanceId = PG.id
		WHERE PG.gID = prm_gameID
			AND PA.resetNumber = loc_reset
			AND PA.roundNumber = loc_round
		GROUP BY playerAction
		ORDER BY playerAction ASC;
END//

CREATE PROCEDURE intf_getPlayerGameMoveSelectionState(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to allow client to determine if a player is "working", "waiting" or "out" during Selection state
	Input:
		An id for the game to retrieve values for
	Output:
		Rows of 
			username
			String value
			state
			Integer value
			if the player has made a selection
			Boolean value
	*/
	SELECT 	P.uName AS player_name,
			PG.playerState AS player_state,
			EXISTS
				(
					SELECT *
						FROM tbl_player_actions
						WHERE playerInstanceID = PG.id
							AND resetNumber = func_currentResetNumber(prm_gameID)
							AND roundNumber = func_currentRoundNumber(prm_gameID)
				) AS player_hasMoved
		FROM tbl_player_game AS PG
		INNER JOIN tbl_player AS P
			ON PG.pID = P.id
		WHERE PG.gID = prm_gameID;
END//

CREATE PROCEDURE intf_getPlayerGameState(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to allow client to retrieve state information about players in a game
	Input:
		An id for the game to retrieve values for
	Output:
		Player Username and State tuples
	*/
	SELECT P.uName AS player_name, PG.playerState AS player_state
		FROM tbl_player_game AS PG
		INNER JOIN tbl_player AS P
			ON PG.pID = P.id
		WHERE PG.gID = prm_gameID
			AND PG.playerState != 2;
END//

CREATE PROCEDURE intf_getGameState(IN prm_gameID INT)
BEGIN
	/*
	Description:
		Procedure to allow a client to see the state of a game
	Input:
		An id for the game to retrieve values for
	Output:
		The state of the game
		Integer value
	*/
	SELECT func_getGameState(prm_gameID) AS game_state;
END//

CREATE PROCEDURE intf_playerActionRegister(IN prm_instID INT, IN prm_action TINYINT)
BEGIN
	/*
	Description:
		Procedure to allow registration of a player's chosen action from a client
	Input:
		player instance id
		action id
	Output:
		The success of the action
		Boolean value
	*/
	-- Local variable declaration
	DECLARE loc_gameID INT; -- Variable to store the game id for this player instance

	-- Error handling declaration
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		SELECT FALSE AS `result`;
	END;
	-- Get game id
	SET loc_gameID =
		(
			SELECT gID
				FROM tbl_player_game
				WHERE id = prm_instID
		);
	
	START TRANSACTION;
	IF ((SELECT gameState FROM tbl_game WHERE id = loc_gameID) = 2) THEN
		-- If the game can accept new actions at this time
		INSERT INTO tbl_player_actions (playerInstanceID, resetNumber, roundNumber, playerAction)
			VALUES
				(
					prm_instID,
					func_currentResetNumber(loc_gameID),
					func_currentRoundNumber(loc_gameID),
					prm_action
				);
	END IF;
	-- Update the game state if all active players have made a selection
	IF
		(
			(
				-- Select subquery to get the number of players who have made an action this reset+round
				SELECT COUNT(PA.playerInstanceID)
					FROM tbl_player_actions AS PA
					INNER JOIN tbl_player_game AS PG
						ON PA.playerInstanceID = PG.id
					WHERE PG.gID = loc_gameID
						AND PG.playerState = 1
						AND PA.resetNumber = func_currentResetNumber(loc_gameID)
						AND PA.roundNumber = func_currentRoundNumber(loc_gameID)
			)
			=
			(
				-- Select subquery to get the number of active players in the game
				SELECT COUNT(id)
					FROM tbl_player_game
					WHERE gID = loc_gameID
						AND playerState = 1
			)
		)
	THEN
		CALL proc_updateGameState(loc_gameID);
	END IF;
	COMMIT;
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_playerDropout(IN prm_instanceID INT)
BEGIN
	/*
	Description:
		Procedure to allow removal of a player from a game room using a client
	Input:
		The player ID
	Output:
		The success of the removal
		Boolean value
	*/
	-- Local variable declaration
	DECLARE loc_gameID INT;
		
	-- Error handling declaration
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		SELECT FALSE AS `result`;
	END;
	SET loc_gameID =
		(
			-- Select subquery that gets the game ID
			SELECT gID
				FROM tbl_player_game
				WHERE id = prm_instanceID
		);
	START TRANSACTION;
	CALL proc_playerDropout(prm_instanceID);
	COMMIT;
	SELECT TRUE AS `result`;
END//

CREATE PROCEDURE intf_checkAccountLocked(IN prm_id INT)
BEGIN
	/*
	Description:
		Procedure that returns the locked state of an account to a client
	Input:
		The account id
	Output:
		The isLocked value
		Boolean
	*/
	SELECT isLocked
		FROM tbl_player
		WHERE id = prm_id;
END//

CREATE PROCEDURE intf_checkAccountOnline(IN prm_id INT)
BEGIN
	/*
	Description:
		Procedure that returns the locked state of an account to a client
	Input:
		The account id
	Output:
		The isOnline value
		Boolean
	*/
	SELECT isOnline
		FROM tbl_player
		WHERE id = prm_id;
END//

CREATE PROCEDURE intf_comparePassword(IN prm_id INT, IN prm_pWord VARCHAR(16))
BEGIN
	/*
	Description:
		Procedure to allow a client to compare passwords
	Input:
		The account id
		The password to check
	Output:
		The result of the comparison
		Boolean Value
	*/
	SELECT func_comparePassword(prm_id, prm_pWord) AS `result`;
END//

CREATE PROCEDURE intf_createPlayer(IN prm_uName VARCHAR(16), IN prm_pWord VARCHAR(16))
BEGIN
	/*
	Description:
		Procedure to create a new account from a client
	Input:
		The username of the new player
		The password of the new player
	Output:
		The id of the new account
		Integer value
	*/
	-- Error handling declaration
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		SELECT NULL AS `result`;
	END;
	-- Add new player row
	INSERT INTO tbl_player (uName, pWord)
		VALUES (prm_uName, prm_pWord);
	SELECT LAST_INSERT_ID() AS `result`;
END//

CREATE PROCEDURE intf_findPlayerAccount(IN prm_uName VARCHAR(16))
BEGIN
	/*
	Description:
		Procedure to allow a client to look up player accounts
	Input:
		String to compare
	Output:
		Returns an player id
		Integer value
		Null returns expected
	*/
	SELECT id AS `player_id`
		FROM tbl_player
		WHERE uName LIKE prm_uName
		LIMIT 1; -- uName is unique, limit to 1 just in case
END//

CREATE PROCEDURE intf_playerLogin(IN prm_playerID INT, IN prm_pWord VARCHAR(16))
BEGIN
	/*
	Description:
		Procedure to allow a player to login from the client
	Input:
		The account ID
		String to compare
	Output:
		The success of the login
		Boolean value
	*/
	DECLARE loc_outvar BOOLEAN;
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		SELECT FALSE AS `result`;
	END;
	START TRANSACTION;
	SET loc_outvar = func_comparePassword(prm_playerID, prm_pWord);
	IF(loc_outvar = TRUE) THEN
		-- If the passwords match, log the player in
		UPDATE tbl_player
			SET isOnline = TRUE, failedLoginAttempts = 0
			WHERE id = prm_playerID;
	ELSE
		-- If passwords do not match update the failed login attempts
		CALL proc_updateLoginAttempts(prm_playerID);
	END IF;
	COMMIT;
	SELECT loc_outvar AS `result`;
END//

CREATE EVENT event_maintenanceUpdate
	ON SCHEDULE EVERY 5 SECOND

DO BEGIN
	/*
	Description:
		Event that runs every 5 seconds to make sure games change states
	*/
	-- Local variable declaration
	DECLARE n INT DEFAULT 0;
	DECLARE i INT DEFAULT 0;
	DECLARE idToUpdate INT DEFAULT NULL;
	-- Error handling declaration
	DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
	END;
	-- Set idle players to offline
	START TRANSACTION;
	-- Get number of rows to edit
	SELECT COUNT(id) INTO n
		FROM tbl_player
		WHERE func_playerIsInactive(id) = TRUE;
	-- Reset iterator
	SET i = 0;
	WHILE i < n DO
		-- Find player that is inactive
		SET idToUpdate = 
			(
				SELECT id 
					FROM tbl_player
					WHERE func_playerIsInactive(id) = TRUE
					LIMIT 1 OFFSET i
			);
		-- Log them out
		CALL proc_playerLogout(idToUpdate);
		-- Update iterator
		SET i = i + 1;
	END WHILE;
	COMMIT;
	-- Update the state of running games
	START TRANSACTION WITH CONSISTENT SNAPSHOT;
	-- Get number of rows to edit
	SELECT COUNT(id) INTO n
		FROM tbl_game
		WHERE func_gameUpdateReady(id) = TRUE;
	-- Reset iterator
	SET i = 0;
	WHILE i < n DO
		-- Find game that needs state change
		SET idToUpdate =
			(
				SELECT id
					FROM tbl_game
					WHERE func_gameUpdateReady(id) = TRUE
					LIMIT 1 OFFSET i
			);
		-- Update state
		CALL proc_updateGameState(idToUpdate);
		-- Update iterator
		SET i = i + 1;
	END WHILE;
	COMMIT ;
END//

/*SECTION 4: TEST DATA CREATION AND TEST PROCEDURES*/
CREATE PROCEDURE test_initializeTestUsers()
BEGIN
	/*
	Description:
		Create a set of test users for use in test procedures.
	*/
	INSERT INTO tbl_player (uName, pWord, isAdmin)
		VALUES
			("User-One", "Password1", TRUE),
			("User-Two", "Password1", FALSE),
			("User-Three", "Password1", FALSE),
			("User-Four", "Password1", FALSE),
			("User-Five", "Password1", FALSE),
			("User-Six", "Password1", FALSE),
			("User-Seven", "Password1", FALSE),
			("User-Eight", "Password1", FALSE),
			("User-Nine", "Password1", FALSE),
			("User-Ten", "Password1", FALSE),
			("User-Eleven", "Password1", FALSE),
			("User-Twelve", "Password1", FALSE),
			("User-Thirteen", "Password1", FALSE),
			("User-Fourteen", "Password1", FALSE),
			("User-Fifteen", "Password1", FALSE);
END//

CREATE PROCEDURE test_dropGameUpdateEvent()
BEGIN
	/*
	Description:
		Gets rid of the event so the game can be tested without it
	*/
	DROP EVENT IF EXISTS event_maintenanceUpdate;
END//

CREATE PROCEDURE test_phaseOne()
BEGIN
	/*
	Description:
		Run tests for account creation and player logging in/out
	*/
	CALL test_initializePhaseOne();
	CALL test_findPlayerAccount();
	CALL test_accountLocked();
	CALL test_accountAdmin();
	CALL test_passwordCheck();
	CALL test_playerLogin();
	CALL test_playerLogout();
	CALL test_accountCreation();
END//

CREATE PROCEDURE test_initializePhaseOne()
BEGIN
	/*
	Description:
		Gets the database ready to run tests for account creation and player logging in/out
	*/
	-- Remove the scheduled event
	CALL test_dropGameUpdateEvent();
	-- Create fresh tables
	CALL proc_initializeTables();
	-- Create test users
	CALL test_initializeTestUsers();
END//

CREATE PROCEDURE test_phaseTwo()
BEGIN
	/*
	Description:
		Run tests for game creation, joining, leaving and information viewing
	*/
	CALL test_initializePhaseTwo();
	CALL test_gameCreation();
	CALL test_gameJoining();
	CALL test_gameLeaving();
	CALL test_gameList();
	CALL test_getHostName();
	CALL test_getPlayerCount();
END//

CREATE PROCEDURE test_initializePhaseTwo()
BEGIN
	/*
	Description:
		Gets the database ready to run tests for game creation, joining, leaving and information viewing
	*/
	DECLARE i INT DEFAULT 1;
	-- Remove the scheduled event
	CALL test_dropGameUpdateEvent();
	-- Create fresh tables
	CALL proc_initializeTables();
	-- Create test users
	CALL test_initializeTestUsers();
	-- Log all users in
	WHILE i < 16 DO
		CALL intf_playerLogin(i, "Password1");
		SET i = i + 1;
	END WHILE;
END//

CREATE PROCEDURE test_phaseThree()
BEGIN
	/*
	Description:
		Run tests for the running of a game.
	*/
	CALL test_initializePhaseThree();
	CALL test_gameUpdateCheck();
	CALL test_gameUpdate();
	CALL test_resetNumber();
	CALL test_roundNumber();
	CALL test_gameState();
	CALL test_playerGameMadeMove();
	CALL test_countPerAction();
	CALL test_gameplayCaseOne();
	CALL test_gameplayCaseTwo();
	CALL test_gameplayCaseThree();
	CALL test_gameplayCaseFour();
	CALL test_gameplayCaseFive();
END//

CREATE PROCEDURE test_initializePhaseThree()
BEGIN
	/*
	Description:
		Gets the database ready to run tests for the running of a game.
	*/
	DECLARE i INT DEFAULT 1;
	DECLARE j INT;
	-- Remove the scheduled event
	CALL test_dropGameUpdateEvent();
	-- Create fresh tables
	CALL proc_initializeTables();
	-- Create test users
	CALL test_initializeTestUsers();
	-- Log all users in
	WHILE i < 16 DO
		CALL intf_playerLogin(i, "Password1");
		SET i = i + 1;
	END WHILE;
	CALL intf_initiateGame(1);
	SET i = 2;
	WHILE i < 7 DO
		SET j = func_newPlayerInstance(i, 1);
		SET i = i + 1;
	END WHILE;
END//

CREATE PROCEDURE test_phaseThreeGameReset()
BEGIN
	/*
	Description:
		Resets the game so that a new case can be tested
	*/
	SET foreign_key_checks = 0;
	-- Reset the game
	UPDATE tbl_game
		SET
			gameState = 2,
			resetNumber = 0,
			roundNumber = 1,
			stateStartTime = NOW();
	-- Reset the players
	UPDATE tbl_player_game
		SET playerState = 1;
	-- Reset the inputs
	DELETE FROM tbl_player_actions;
	SET foreign_key_checks = 1;
END//

CREATE PROCEDURE test_phaseFour()
BEGIN
	/*
	Description:
		Run tests for admin functionality.
	*/
	CALL test_initializePhaseFour();
	CALL test_endGame();
	CALL test_userList();
	CALL test_userInfo();
	CALL test_userUpdate();
	CALL test_userDelete();
	CALL test_userInactiveCheck();
END//

CREATE PROCEDURE test_initializePhaseFour()
BEGIN
	/*
	Description:
		Gets the database ready to run tests for admin functionality
	*/
	DECLARE i INT DEFAULT 1;
	-- Remove the scheduled event
	CALL test_dropGameUpdateEvent();
	-- Create fresh tables
	CALL proc_initializeTables();
	-- Create test users
	CALL test_initializeTestUsers();
	-- Log all users in
	WHILE i < 16 DO
		CALL intf_playerLogin(i, "Password1");
		SET i = i + 1;
	END WHILE;
	SET i = 0;
	WHILE i < 4 DO 
		CALL intf_initiateGame(i);
		SET i = i + 1;
	END WHILE;
END//

-- Phase 1 procedures

CREATE PROCEDURE test_findPlayerAccount()
BEGIN
	/*
	Description:
		Tests the "Find Player Account by Username" activity
	Expected Result:
		The id of the exisitng user
			player_id: 13
	*/
	CALL intf_findPlayerAccount("User-Thirteen");
END//

CREATE PROCEDURE test_accountLocked()
BEGIN
	/*
	Description:
		Tests the "Check If a User Account is Locked" activity
	Expected Result:
		The isLocked value
			is_locked: 0
	*/
	CALL intf_checkAccountLocked(2);
END//

CREATE PROCEDURE test_accountAdmin()
BEGIN
	/*
	Description:
		Tests the "Check Account has Admin Privileges" activity
	Expected Result:
		The isAdmin value
			is_admin: 1
	*/
	CALL intf_isPlayerAdmin(1);
END//

CREATE PROCEDURE test_passwordCheck()
BEGIN
	/*
	Description:
		Tests the "Check Password is Correct" activity
	Expected Result:
		The success of the action
			password_correct: 1
	*/
	CALL intf_comparePassword(1, "Password1");
END//

CREATE PROCEDURE test_playerLogin()
BEGIN
	/*
	Description:
		Tests the "Player Logs In" activity
	Expected Result:
		The affected rows of the player table "User-One", "User-Three" and "User-Four" logged in, "User-Two" locked out.
	*/
	DECLARE i INT DEFAULT 0;
	DECLARE loc_result BOOLEAN;
	CALL intf_playerLogin(1, "Password1");
	CALL intf_playerLogin(3, "Password1");
	CALL intf_playerLogin(4, "Password1");
	WHILE i < 5 DO
		CALL intf_playerLogin(2, "Password");
		SET i = i + 1;
	END WHILE;
	SELECT * FROM tbl_player WHERE id IN (1,2,3,4);
END//

CREATE PROCEDURE test_playerLogout()
BEGIN
	/*
	Description:
		Tests the "User Logout" activity
	Expected Result:
		The same rows of the player table "User-Four" is now logged out though
	*/
	CALL intf_playerLogout(4);
	SELECT * FROM tbl_player WHERE id IN (1,2,3,4);
END//

CREATE PROCEDURE test_accountCreation()
BEGIN
	/*
	Description:
		Tests the "New Account Created" activity
	Expected Result:
		The newly created row from the player table, "User-Sixteen"
	*/
	CALL intf_createPlayer("User-Sixteen", "Password1");
	SELECT * FROM tbl_player WHERE uName = "User-Sixteen";
END//

-- Phase 2 procedures

CREATE PROCEDURE test_gameCreation()
BEGIN
	/*
	Description:
		Tests the "Create New Game Room" activity
	Expected Result:
		The created rows from the game table
	*/
	CALL intf_initiateGame(1);
	CALL intf_initiateGame(2);
	CALL intf_initiateGame(3);
	SELECT * FROM tbl_game;
END//

CREATE PROCEDURE test_gameJoining()
BEGIN
	/*
	Description:
		Tests the "Player Joins a Game Room" activity
	Expected Result:
		All the instance of players in game
	*/
	DECLARE i INT;
	SET i = func_newPlayerInstance(4, 1);
	SET i = func_newPlayerInstance(5, 1);
	SET i = func_newPlayerInstance(6, 1);
	SET i = func_newPlayerInstance(4, 2);
	SET i = func_newPlayerInstance(5, 2);
	SET i = func_newPlayerInstance(6, 2);
	SET i = func_newPlayerInstance(7, 3);
	SET i = func_newPlayerInstance(8, 3);
	SET i = func_newPlayerInstance(9, 3);
	SELECT * FROM tbl_player_game ORDER BY gID ASC;
END//

CREATE PROCEDURE test_gameLeaving()
BEGIN
	/*
	Description:
		Tests the "Player Leaves a Game Room" activity
		and the "Game Room Finds a New Host" activity
	Expected Result:
		All the instance of players in game updated to reflect the changes to the playerState and isHost values
	*/
	CALL proc_playerDropout(3);
	SELECT * FROM tbl_player_game ORDER BY gID ASC;
END//

CREATE PROCEDURE test_gameList()
BEGIN
	/*
	Description:
		Tests the "Get List of Active Games" activity
	Expected Result:
		All rows for active games.
	*/
	CALL intf_getRunningGames();
END//

CREATE PROCEDURE test_getHostName()
BEGIN
	/*
	Description:
		Tests the "Get Game Host Name" activity
	Expected Result:
		The host name of game 1
			host_name: "User-One"
	*/
	SELECT func_getHostName(1) AS `host_name`;
END//

CREATE PROCEDURE test_getPlayerCount()
BEGIN
	/*
	Description:
		Tests the "Get Number of Players in a Game" activity
	Expected Result:
		The count of players in game 13
			player_count: 4
	*/
	SELECT func_getPlayerCount(1) AS player_count;
END//

-- Phase 3 procedures

CREATE PROCEDURE test_gameUpdateCheck()
BEGIN
	/*
	Description:
		Tests the "Check if Game Needs Updating" activity
	Expected Result:
		Whether or not the game needs to be updated
			update_game: 1
	*/
	SELECT func_gameUpdateReady(1) AS update_game;
END//

CREATE PROCEDURE test_gameUpdate()
BEGIN
	/*
	Description:
		Tests the "Update a Game’s State" activity
	Expected Result:
		The game row, showing the game in the "Selecting" state
	*/
	DECLARE i INT DEFAULT 0;
	WHILE i < 2 DO
		CALL proc_updateGameState(1);
		SET i = i + 1;
	END WHILE;
	SELECT * FROM tbl_game;
END//

CREATE PROCEDURE test_resetNumber()
BEGIN
	/*
	Description:
		Tests the "Get a Game’s Reset Number" activity
	Expected Result:
		The reset number for the game
			reset_number: 0
	*/
	SELECT func_currentResetNumber(1) AS reset_number;
END//

CREATE PROCEDURE test_roundNumber()
BEGIN
	/*
	Description:
		Tests the "Get a Game’s Round Number" activity
	Expected Result:
		The reset number for the game
			reset_number: 0
	*/
	SELECT func_currentRoundNumber(1) AS round_number;
END//

CREATE PROCEDURE test_gameState()
BEGIN
	/*
	Description:
		Tests the "Check a Game’s State" activity
	Expected Result:
		The state value for the game.
			game_state: 2
	*/
	SELECT func_getGameState(1) AS game_state;
END//

CREATE PROCEDURE test_playerGameState()
BEGIN
	/*
	Description:
		Tests the "Get State of Players in a Game" activity
	Expected Result:
		All the player names in the game and their state
	*/
	CALL intf_getPlayerGameState(1);
END//

CREATE PROCEDURE test_playerGameMadeMove()
BEGIN
	/*
	Description:
		Tests the "Check if Players Have Made a Move" activity
	Expected Result:
		All the player names in the game and a value to show if they've entered a move or not
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,1);
	CALL intf_playerActionRegister(2,0t);
	CALL intf_getPlayerGameMoveSelectionState(1);
END//

CREATE PROCEDURE test_countPerAction()
BEGIN
	/*
	Description:
		Tests the "Get Count of Players That Chose Each Action" activity
	Expected Result:
		All actions and the number of players that chose them
		0: 1
		1: 2 
		2: 3
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,0);
	CALL intf_playerActionRegister(2,1);
	CALL intf_playerActionRegister(3,1);
	CALL intf_playerActionRegister(4,2);
	CALL intf_playerActionRegister(5,2);
	CALL intf_playerActionRegister(6,2);
	CALL intf_getCountPerAction(1);
END//

CREATE PROCEDURE test_gameplayCaseOne()
BEGIN
	/*
	Description:
		Runs a game round where only one action is played
		Tests the "Losing Action is Calculated" activity
		and the "Get Number of Actions Played" activity
		and the "Find the Weaker of Two Actions" activity
	Expected Result:
		The losing action
			losing_action: NULL
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,1);
	CALL intf_playerActionRegister(2,1);
	CALL intf_playerActionRegister(3,1);
	SELECT func_calculateLosingAction(1, 0, 1) AS losing_action;
END//

CREATE PROCEDURE test_gameplayCaseTwo()
BEGIN
	/*
	Description:
		Runs a game round where two actions are played
		Tests the "Losing Action is Calculated" activity
		and the "Get Number of Actions Played" activity
		and the "Find the Weaker of Two Actions" activity
	Expected Result:
		The losing action
			losing_action: 0
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,0);
	CALL intf_playerActionRegister(2,1);
	SELECT func_calculateLosingAction(1, 0, 1) AS losing_action;
END//

CREATE PROCEDURE test_gameplayCaseThree()
BEGIN
	/*
	Description:
		Runs a game round where three actions are played with only one having the most votes
		Tests the "Losing Action is Calculated" activity
		and the "Get Number of Actions Played" activity
		and the "Get Number of Actions with the Most Votes" activity
		and the "Find the Weaker of Two Actions" activity
	Expected Result:
		The losing action
			losing_action: 1
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,0);
	CALL intf_playerActionRegister(2,1);
	CALL intf_playerActionRegister(3,2);
	CALL intf_playerActionRegister(4,2);
	SELECT func_calculateLosingAction(1, 0, 1) AS losing_action;
END//

CREATE PROCEDURE test_gameplayCaseFour()
BEGIN
	/*
	Description:
		Runs a game round where three actions are played with two having the most votes
		Tests the "Losing Action is Calculated" activity
		and the "Get Number of Actions Played" activity
		and the "Get Number of Actions with the Most Votes" activity
		and the "Find the Weaker of Two Actions" activity
	Expected Result:
		The losing action
			losing_action: 2
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,0);
	CALL intf_playerActionRegister(2,0);
	CALL intf_playerActionRegister(3,2);
	CALL intf_playerActionRegister(4,2);
	CALL intf_playerActionRegister(5,1);
	SELECT func_calculateLosingAction(1, 0, 1) AS losing_action;
END//

CREATE PROCEDURE test_gameplayCaseFive()
BEGIN
	/*
	Description:
		Runs a game round where three actions are played with three having the most votes
		Tests the "Losing Action is Calculated" activity
		and the "Get Number of Actions Played" activity
		and the "Get Number of Actions with the Most Votes" activity
		and the "Find the Weaker of Two Actions" activity
	Expected Result:
		The losing action
			losing_action: NULL
	*/
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,0);
	CALL intf_playerActionRegister(2,0);
	CALL intf_playerActionRegister(3,2);
	CALL intf_playerActionRegister(4,2);
	CALL intf_playerActionRegister(5,1);
	CALL intf_playerActionRegister(6,1);
	SELECT func_calculateLosingAction(1, 0, 1) AS losing_action;
END//

CREATE PROCEDURE test_updateWinStreak()
BEGIN
	/*
	Description:
		Tests the "Update a Player’s Win Streak" activity
	Expected Result:
		A player row with an updated win streak
	*/
	DECLARE i INT DEFAULT 0;
	CALL test_phaseThreeGameReset();
	CALL intf_playerActionRegister(1,0);
	CALL intf_playerActionRegister(2,1);
	WHILE i < 3 DO
		CALL proc_updateGameState(1);
		SET i = i + 1;
	END WHILE;
	CALL intf_getPlayerInfo(2);
END//

-- Phase 4 procedures

CREATE PROCEDURE test_endGame()
BEGIN
	/*
	Description:
		Tests the "Game Room Ends" activity
	Expected Result:
		The rows from the game table, with game 2's gameState as 6
	*/
	CALL intf_endGame(2);
	SELECT * FROM tbl_game;
END//

CREATE PROCEDURE test_userList()
BEGIN
	/*
	Description:
		Tests the "Get List of All Players" activity
	Expected Result:
		All rows from the player table
	*/
	CALL intf_getPlayerList();
END//

CREATE PROCEDURE test_userInfo()
BEGIN
	/*
	Description:
		Tests the "Get Information About One Player" activity
	Expected Result:
		A single player row
	*/
	CALL intf_getPlayerInfo(4);
END//

CREATE PROCEDURE test_userUpdate()
BEGIN
	/*
	Description:
		Tests the "Admin User Updates a User’s Information" activity
	Expected Result:
		A single player row with updated values
	*/
	CALL intf_updatePlayer(4, "Updated-User", "Password2", 4, 4, TRUE, FALSE);
	CALL intf_getPlayerInfo(4);
END//

CREATE PROCEDURE test_userDelete()
BEGIN
	/*
	Description:
		Tests the "Admin User Deletes a User" activity
	Expected Result:
		A list of all players with some deleted
	*/
	DECLARE i INT DEFAULT 2;
	WHILE i < 16 DO
		CALL intf_deletePlayer(i);
		SET i = i + 2;
	END WHILE;
	CALL intf_getPlayerList();
END//

CREATE PROCEDURE test_userInactiveCheck()
BEGIN
	/*
	Description:
		Tests the "Check for Inactive Users" activity
	Expected Result:
		If player 1 is inactive
			player_inactive: 0
	*/
	SELECT func_playerIsInactive(1) AS player_inactive;
END//
-- Reset delimiter
DELIMITER ;