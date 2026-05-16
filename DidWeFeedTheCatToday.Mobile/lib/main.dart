import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'providers/pet_provider.dart';
import 'screens/pet_list_screen.dart';
import 'screens/login_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final authProvider = AuthProvider();
  await authProvider.checkAuthStatus();

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProxyProvider<AuthProvider, PetProvider>(
        create: (_) => PetProvider(),
        update: (context, auth, pet) {
          return pet!..updateToken(auth.token);
        },),
      ],
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    return MaterialApp(
      title: 'Pet Tracker',
      theme: ThemeData(primarySwatch: Colors.blue, useMaterial3: true),
      home: auth.isAuthenticated 
          ? const PetListScreen() 
          : const LoginScreen(),
    );
  }
}