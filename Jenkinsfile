pipeline {
    agent any
 
    stages {
        stage('Build') {
            steps {
                dir('src') {
                  sh "dotnet build --configuration Release"
                }
            }
        }
        stage('Test-Build') {
            steps {
                dir('src') {
                  sh "dotnet test --configuration Release"
                }
            }
        }
        stage('Publish') {
            steps {
                dir('src') {
                  sh "dotnet publish -c Release -o WebChess"
                }
            }
        }
        stage('Deploy') {
            steps {
                sshPublisher(
                    publishers: [
                        sshPublisherDesc(
                            configName: 'Deploy_server_to_TEST',
                            transfers: [
                                sshTransfer(
                                    cleanRemote: false,
                                    excludes: '',
                                    execCommand: 'sudo systemctl restart chess.service',
                                    execTimeout: 120000,
                                    flatten: false,
                                    makeEmptyDirs: false,
                                    noDefaultExcludes: false,
                                    patternSeparator: '[, ]+',
                                    remoteDirectory: 'WebChess',
                                    remoteDirectorySDF: false,
                                    removePrefix: '',
                                    sourceFiles: 'src/WebChess/*'
                                )
                            ],
                            usePromotionTimestamp: false,
                            useWorkspaceInPromotion: false,
                            verbose: false
                        )
                    ]
                )
            }
        }
    }
}
