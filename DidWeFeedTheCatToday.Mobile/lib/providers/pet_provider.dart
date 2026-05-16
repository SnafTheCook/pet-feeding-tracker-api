import 'package:flutter/widgets.dart';
import 'package:pet_api_mobile/models/pet_dto.dart';
import 'package:pet_api_mobile/models/result.dart';
import 'package:pet_api_mobile/services/api_service.dart';

class PetProvider extends ChangeNotifier {
  final ApiService _apiService = ApiService();

  List<PetDto> _pets = [];
  bool _isLoading = false;
  String? _errorMessage;
  PetDto? _selectedPet;
  String? _token;

  List<PetDto> get pets => _pets;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  PetDto? get selectedPet => _selectedPet;

  Future<void> refreshPets() async
  {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _apiService.getPets(_token);
    switch (result) {
      case Success(data: var petList):
        _pets = petList;
        _errorMessage = null;
        break;
      case Failure(message: String msg):
        _errorMessage = msg;
        break;
    }

    _isLoading = false;
    notifyListeners();

  }

  Future<Result> feedPet(int petId) async
  {
    final result = await _apiService.postFeeding(petId);
    
    switch (result) {
      case Success(data: var petData):
        refreshPets();
        return Success(petData);
      case Failure(message: var msg):
        debugPrint(msg);
        refreshPets();
        return Failure(msg);
    }
  }

  Future<Result> addPet(String name, int? age, String? info) async {
    final result = await _apiService.addPet(name, age, info);

    switch (result) {
      case Success(data: var petData):
        refreshPets();
        return Success(petData);
      case Failure(message: var msg):
        debugPrint(msg);
        refreshPets();
        return Failure(msg);
      }
  }

  Future<void> loadPetDetails(int id) async {
    _isLoading = true;
    _selectedPet = null;
    notifyListeners();

    final result = await _apiService.getPetById(id);

    switch (result) {
      case Success(data: var pet):
        _selectedPet = pet;
      case Failure(message: var msg):
        _errorMessage = msg;
    }

    _isLoading = false;
    notifyListeners();
  }

  void updateToken(String? newToken) {
    _token = newToken;
  }
}