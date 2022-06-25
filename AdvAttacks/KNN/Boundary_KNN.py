'''
Версия python 3.8.10
используемые версии модулей python:
pandas==1.3.4
numpy==1.20.2
matplotlib==3.4.1
scikit-learn==0.22.2
tensorflow==2.8.0
keras==2.8.0
adversarial-robustness-toolbox==1.10.0
'''

#импорт необходимых модулей
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import KNeighborsClassifier
from sklearn.metrics import classification_report, confusion_matrix
from tensorflow.keras.utils import to_categorical
from art.attacks.evasion import BoundaryAttack
from art.estimators.classification import BlackBoxClassifierNeuralNetwork

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

#определение классификатора
classifier = KNeighborsClassifier(n_neighbors=3)

#функция получения предсказаний
def get_predict(test_df):
    test_df = test_df.reshape(test_df.shape[0], 144)
    y_pred = classifier.predict(test_df)
    result = to_categorical(y_pred, 23)
    return result

#определение атаки
classifier_nn = BlackBoxClassifierNeuralNetwork(predict_fn = get_predict, 
                                    nb_classes = 23, 
                                    input_shape=(144,))
attack = BoundaryAttack(estimator = classifier_nn, max_iter=1, verbose = False)

#проведение атаки и сохранение результатов
accuracy_list = []

for i in range(30):
    print('Iteration #' + str(i + 1))
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.50)
    
    scaler = StandardScaler()
    scaler.fit(X_train)
    X_train = scaler.transform(X_train)
    X_test = scaler.transform(X_test)
    classifier = KNeighborsClassifier(n_neighbors=3)
    classifier.fit(X_train, y_train)
    
    X_test = X_test[:113]
    y_test = y_test[:113]
    
    y_pred = classifier.predict(X_test)
    accuracy_before = np.mean(y_pred == y_test)
    print('Accuracy before attack: ', accuracy_before)
    
    y_test_gen = to_categorical(y_test, 23)
    X_test_adv = attack.generate(x=X_test, y = y_test_gen)
    
    y_pred = classifier.predict(X_test_adv)
    accuracy_after = np.mean(y_pred == y_test)
    print('Accuracy after attack: ', accuracy_after)
    
    accuracy_list.append([accuracy_before, accuracy_after])

acc_df = pd.DataFrame(accuracy_list, columns=['Before', 'After'])
acc_df.to_csv('BoundaryAttack_KNN.csv')