node {
	stage 'Checkout'
		checkout scm
		bat 'git submodule update --init'

	stage 'Build'
		bat 'nuget restore "NecroBot.sln"'
		bat "\"${tool 'MSBuild'}\" NecroBot.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.1.${env.BUILD_NUMBER}"

	stage 'Archive'
		archive 'PoGo.NecroBot.CLI/bin/Release/**'
		archiveArtifacts artifacts: 'PoGo.NecroBot.CLI/bin/Release/', fingerprint: true, onlyIfSuccessful: true
		bat 'copy "PoGo.NecroBot.CLI/bin/Release/Necrobot2.exe" "d:\J\Out" /Y'
		bat 'copy "PoGo.NecroBot.WIN/bin/Release/Necrobot2.Win.exe" "d:\J\Out" /Y'
	
}