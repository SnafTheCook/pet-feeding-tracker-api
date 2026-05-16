import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:pet_api_mobile/models/result.dart';
import '../models/pet_dto.dart';

class ApiService {
  final String _baseUrl = "http://localhost:8080/api"; 

  Future<Result<List<PetDto>>> getPets(String? token) async
  {
    try {
      final response = await http.get(
        Uri.parse('$_baseUrl/pets'),
        headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },);

      if (response.statusCode == 200) {
        final Map<String, dynamic> rootJson = jsonDecode(response.body);
        final List<dynamic> items = rootJson['data']['items'];

        return Success(items.map((json) => PetDto.fromJson(json)).toList());
      }
      else {
        return Failure('Server returned code: ${response.statusCode}');
      }
      
    } catch (e) {
      return Failure('Network error: $e');
    }
  }

  Future<Result<PetDto>> getPetById(int id) async {
  try {
    final response = await http.get(Uri.parse('$_baseUrl/pets/$id'));
    final Map<String, dynamic> envelope = jsonDecode(response.body);

    if (response.statusCode == 200) {
      final petData = envelope['data']; 
      return Success(PetDto.fromJson(petData));
    }
    return Failure(envelope['error'] ?? "Pet not found");
  } catch (e) {
    return Failure("System Error: $e");
  }

  }

  Future<Result<bool>> postFeeding(int petId) async
  {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/feedings'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'petId': petId,
          'feedingTime': DateTime.now().toUtc().toIso8601String()
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        return Success(true);
      }
      else if (response.statusCode == 401) {
        return Failure('Your session has expired. Please log in again.');
      }
      else {
        return Failure('Server error: ${response.statusCode}');
      }
    } catch (e) {
      return Failure('Check your internet connection');
    }
  }

  Future<Result<bool>> addPet(String name, int? age, String? info) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/pets'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'name': name,
          'age': age,
          'additionalInformation': info
        })
      );
      if (response.statusCode == 200 || response.statusCode == 201) {
        return Success(true);
      }
      else if (response.statusCode == 401) {
        return Failure('Your session has expired. Please log in again.');
      }
      else {
        return Failure('Server error: ${response.statusCode}');
      }
    } catch (e) {
      return Failure('Check your internet connection');
    }
  }
}