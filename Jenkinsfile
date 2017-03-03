node {
	stage 'Checkout'
		checkout scm


	stage 'Build'
		bat 'nuget restore "NecroBot-Private for Pokemon GO.sln"'
		bat "\"${tool 'MSBuild'}\" "NecroBot-Private for Pokemon GO.sln" /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.1.${env.BUILD_NUMBER}"

	stage 'Archive'
		archive 'ProjectName/bin/Release/**'

}