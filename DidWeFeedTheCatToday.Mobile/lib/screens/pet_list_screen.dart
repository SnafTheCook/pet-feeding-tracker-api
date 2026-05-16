import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../providers/pet_provider.dart';
import '../widgets/pet_card.dart';

class PetListScreen extends StatefulWidget {
  const PetListScreen({super.key});

  @override
  State<PetListScreen> createState() => _PetListScreenState();
}

class _PetListScreenState extends State<PetListScreen> {
  @override
  void initState() {
    super.initState();
    
    Future.microtask(() { 
      if (!mounted) return;
      context.read<PetProvider>().refreshPets();
      });
  }

  @override
  Widget build(BuildContext context) {
    final petProvider = context.watch<PetProvider>();

    return Scaffold(
      appBar: AppBar(
        title: const Text("My Pets"),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => context.read<AuthProvider>().logout(),
          )
        ],
      ),
      body: petProvider.isLoading 
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              itemCount: petProvider.pets.length,
              itemBuilder: (context, index) => PetCard(pet: petProvider.pets[index], onFeed: () {}),
            ),
    );
  }
}