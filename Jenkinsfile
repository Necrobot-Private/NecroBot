node {
	stage 'Test'
	echo $OUTPUT_PATH
	
	stage 'Checkout'
		checkout scm
		bat 'git submodule update --init'

	stage 'Build'
		bat 'nuget restore "NecroBot-Private for Pokemon GO.sln"'
		bat "\"${tool 'MSBuild'}\" \"NecroBot-Private for Pokemon GO.sln\" /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.1.${env.BUILD_NUMBER}"

	stage 'Archive'
		archive 'PoGo.NecroBot.CLI/bin/Release/**'
		archiveArtifacts artifacts: 'PoGo.NecroBot.CLI/bin/Release/', fingerprint: true, onlyIfSuccessful: true
		bat 'copy "PoGo.NecroBot.CLI\\bin\\Release\\Necrobot2.exe" "${OUTPUT_PATH}" /Y'
		bat 'copy "PoGo.Necrobot.Window\\bin\\Release\\Necrobot2.Win.exe" "${OUTPUT_PATH}" /Y'
		bat '"7z.exe" a ${OUTPUT_PATH}\\Necrobot.CLI.zip PoGo.NecroBot.CLI\\bin\\Release\\*.*'
		bat '"7z.exe" a ${OUTPUT_PATH}\\Necrobot.WIN.zip PoGo.NecroBot.Window\\bin\\Release\\*.*'
	}