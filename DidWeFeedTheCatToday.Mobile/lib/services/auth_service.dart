import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:pet_api_mobile/models/result.dart';

class AuthService {
  final String _baseUrl = "http://localhost:8080/api/auth";

  Future<Result<String>> login(String username, String password) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'username': username, 'password': password}),
      );

      if (response.statusCode == 200){
        final Map<String, dynamic> body = jsonDecode(response.body);
        final String token = body['data']['accessToken'];
        return Success(token);
      }
      return Failure('Invalid username or password');
    } catch (e) {
      return Failure("Connection failed");
    }
  }
}