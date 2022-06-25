'''
Версия python 3.8.10
используемые версии модулей python:
pandas==1.3.4
numpy==1.20.2
matplotlib==3.4.1
scikit-learn==0.22.2
'''

#импорт необходимых модулей
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import KNeighborsClassifier
from sklearn.metrics import classification_report, confusion_matrix

#объявление служебных переменных и констант
df_name = '../sign_dump_KNN.csv'
id_user='id_user'

#загрузка датасета
df = pd.read_csv(df_name)
df = df.drop(df.columns[0], axis=1) #удаление служебного безымянного столбца

#подготовка данных для классификатора
dataset = df
X = dataset.drop(columns='id_user', axis=1)
y = dataset['id_user']
X = X.to_numpy()
y = y.to_numpy()

#выбор наилучшего значения K-соседей
error = []
for i in range(1, 40):
    knn = KNeighborsClassifier(n_neighbors=i)
    knn.fit(X_train, y_train)
    pred_i = knn.predict(X_test)
    error.append(np.mean(pred_i != y_test))
    
plt.figure(figsize=(12, 6))
plt.plot(range(1, 40), error, color='red', linestyle='dashed', marker='o',
         markerfacecolor='blue', markersize=10)
plt.title('Error Rate K Value')
plt.xlabel('K Value')
plt.ylabel('Mean Error')

#создание и тестирование классификатора при различных данных в тестовой выборке
accuracy_list = []

for i in range(30):
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.50)
    scaler = StandardScaler()
    scaler.fit(X_train)
    X_train = scaler.transform(X_train)
    X_test = scaler.transform(X_test)
    classifier = KNeighborsClassifier(n_neighbors=3)
    classifier.fit(X_train, y_train)
    y_pred = classifier.predict(X_test)
    accuracy = np.mean(y_pred == y_test)
    print('Iteration #' + str(i + 1) + ' accuracy:')
    print(accuracy)
    accuracy_list.append(accuracy)

acc_df = pd.DataFrame(accuracy_list)
acc_df.to_csv('Basic_KNN.csv')

