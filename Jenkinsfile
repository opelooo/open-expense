pipeline {
    agent any

    environment {
        DB_CONNECTION = credentials('DB_CONNECTION_STRING')
        DOCKER_IMAGE = "accounting-app:latest"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Test') {
            environment {
                ConnectionStrings__DefaultConnection = "${DB_CONNECTION}"
            }
            steps {
                // Testing menggunakan SDK di server Jenkins sebelum masuk Docker
                sh 'dotnet test --verbosity normal'
            }
        }

        stage('Docker Build') {
            steps {
                script {
                    // Membangun image menggunakan Dockerfile Alpine tadi
                    sh "docker build -t ${DOCKER_IMAGE} ."
                }
            }
        }

        stage('Docker Run/Deploy') {
            steps {
                script {
                    // Stop container lama jika ada
                    sh "docker stop accounting-container || true"
                    sh "docker rm accounting-container || true"
                    
                    // Jalankan container baru dengan koneksi DB dari Jenkins
                    sh """
                        docker run -d \
                        --name accounting-container \
                        -p 5191:8080 \
                        -e ConnectionStrings__DefaultConnection='${DB_CONNECTION}' \
                        -e ASPNETCORE_ENVIRONMENT=Production \
                        ${DOCKER_IMAGE}
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
