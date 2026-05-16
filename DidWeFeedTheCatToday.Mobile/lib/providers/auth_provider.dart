import 'package:flutter/material.dart';
import 'package:pet_api_mobile/models/result.dart';
import 'package:pet_api_mobile/services/auth_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AuthProvider extends ChangeNotifier {
  final AuthService _authService = AuthService();

  String? _token;
  bool _isAuthenticated = false;

  String? get token => _token;
  bool get isAuthenticated => _isAuthenticated;

  Future<void> checkAuthStatus() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString('authToken');
    _isAuthenticated = _token != null;
    notifyListeners();
  }

  Future<Result<String>> login(String user, String pass) async {
    final result = await _authService.login(user, pass);

    if (result is Success<String>) {
      _token = result.data;
      _isAuthenticated = true;

      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('authToken', _token!);

      notifyListeners();
    }

    return result;
  }

  Future<void> logout() async {
    _token = null;
    _isAuthenticated = false;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('authToken');
    notifyListeners();
  }
}