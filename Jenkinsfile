pipeline {
    agent any

    environment {
        DB_CONNECTION = credentials('DB_CONNECTION_STRING')
        DOCKER_IMAGE_NAME = 'open-expense'
        DOCKER_TAG = 'latest-arm64'
        REGISTRY_SERVER = 'registry.opeloooco.uk'
        REG_AUTH = credentials('REGISTRY_AUTH')
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        // stage('Test') {
        //     environment {
        //         ConnectionStrings__DefaultConnection = "${DB_CONNECTION}"
        //     }
        //     steps {
        //         // Testing menggunakan SDK di server Jenkins sebelum masuk Docker
        //         sh 'dotnet test --verbosity normal'
        //     }
        // }

        stage('Docker Build') {
            agent { label 'master' }
            steps {
                script {
                    // Gunakan single quotes '' agar password tidak bocor di log
                    // Gunakan REGISTRY_SERVER (sesuai environment di atas)
                    sh 'echo ${REG_AUTH_PSW} | docker login ${REGISTRY_SERVER} -u ${REG_AUTH_USR} --password-stdin'

                    // Build dengan format: registry.opeloooco.uk/open-expense:latest
                    // sh "docker build -t ${REGISTRY_SERVER}/${DOCKER_IMAGE_NAME}:${DOCKER_TAG} ."
                    sh "docker buildx build --platform linux/arm64 -t ${REGISTRY_SERVER}/${DOCKER_IMAGE_NAME}:${DOCKER_TAG} --push ."

                    // Push ke registry
                    sh "docker push ${REGISTRY_SERVER}/${DOCKER_IMAGE_NAME}:${DOCKER_TAG}"
                }
            }
        }

        stage('Docker Run/Deploy') {
            agent { label 'orangepi' }
            steps {
                script {
                    // Login Podman (Gunakan --tls-verify=false jika registry kamu belum HTTPS)
                    sh 'echo ${REG_AUTH_PSW} | podman login ${REGISTRY_SERVER} -u ${REG_AUTH_USR} --password-stdin'

                    // Stop & Remove lama
                    sh 'podman stop open-expense || true'
                    sh 'podman rm open-expense || true'

                    // Pull image ARM64 dari registry
                    sh "podman pull ${REGISTRY_SERVER}/${DOCKER_IMAGE_NAME}:${DOCKER_TAG}"

                    // Run menggunakan Podman
                    sh """
                        podman run -d \
                        --name open-expense \
                        --net=host \
                        -e ASPNETCORE_URLS="http://+:3000" \
                        -e ConnectionStrings__DefaultConnection='${DB_CONNECTION}' \
                        -e ASPNETCORE_ENVIRONMENT=Production \
                        --restart always \
                        ${REGISTRY_SERVER}/${DOCKER_IMAGE_NAME}:${DOCKER_TAG}
                    """
                }
            }
        }
    }

    post {
        always {
            // Kita tidak cleanWs() jika ingin docker cache tetap ada untuk build berikutnya
            echo 'Pipeline finished.'
        }
    }
}
