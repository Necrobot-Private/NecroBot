node {
	stage 'Checkout'
		checkout scm
		bat 'git submodule update --init'

	stage 'Build'
		bat 'nuget restore "NecroBot-Private for Pokemon GO.sln"'
		bat "\"${tool 'MSBuild'}\" \"NecroBot-Private for Pokemon GO.sln\" /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.1.${env.BUILD_NUMBER}"

	stage 'Archive'
		archive 'PoGo.NecroBot.CLI/bin/Release/**'
		archiveArtifacts artifacts: 'PoGo.NecroBot.CLI/bin/Release/', fingerprint: true, onlyIfSuccessful: true
		bat 'copy "PoGo.NecroBot.CLI\\bin\\Release\\Necrobot2.exe" "d:\\J\\Out\\" /Y'
		bat 'copy "PoGo.Necrobot.Window\\bin\\Release\\Necrobot2.Win.exe" "d:\\J\\Out\\" /Y'
		bat '"C:\\Program Files\\7-Zip\\7z.exe" a d:\\j\\out\\Necrobot.CLI.zip PoGo.NecroBot.CLI\\bin\\Release\\*.*'
		bat '"C:\\Program Files\\7-Zip\\7z.exe" a d:\\j\\out\\Necrobot.WIN.zip PoGo.NecroBot.Window\\bin\\Release\\*.*'
	}