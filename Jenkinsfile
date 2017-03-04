node {
	stage 'Checkout'
		checkout scm
		bat 'git submodule update --init'

	stage 'Build'
		bat 'nuget restore "NecroBot.sln"'
		bat "\"${tool 'MSBuild'}\" NecroBot.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.1.${env.BUILD_NUMBER}"

	stage 'Archive'
	{
		archive 'PoGo.NecroBot.CLI/bin/Release/**'
		archiveArtifacts artifacts: 'PoGo.NecroBot.CLI/bin/Release/*.*', fingerprint: true
	}

}