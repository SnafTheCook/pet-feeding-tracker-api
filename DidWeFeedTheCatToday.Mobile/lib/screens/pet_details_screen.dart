import 'package:flutter/material.dart';
import 'package:pet_api_mobile/models/pet_dto.dart';
import 'package:pet_api_mobile/providers/pet_provider.dart';
import 'package:provider/provider.dart';

class PetDetailsScreen extends StatefulWidget {
  final int petId;
  final String petName;

  const PetDetailsScreen({super.key, required this.petId, required this.petName});

  @override
  State<PetDetailsScreen> createState() => _PetDetailsScreenState();
}

class _PetDetailsScreenState extends State<PetDetailsScreen> {
  
  @override
  void initState() {
    super.initState();

    Future.microtask((){
      if (!mounted) return;
      context.read<PetProvider>().loadPetDetails(widget.petId);
    });
  }

  @override
  Widget build(BuildContext context) {
    final provider = context.watch<PetProvider>();
    final pet = provider.selectedPet;

    return Scaffold(
      appBar: AppBar(title: Text(widget.petName)),
      body: provider.isLoading 
        ? const Center(child: CircularProgressIndicator())
        : pet == null 
          ? const Center(child: Text("Pet not found"))
          : _buildDetails(pet),
    );
  }

  Widget _buildDetails(PetDto pet) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
         Text("ID: ${pet.id}"),
         Text("Name: ${pet.name}", style: const TextStyle(fontSize: 24)),
         const Divider(),
         const Text("Full Bio:", style: TextStyle(fontWeight: FontWeight.bold)),
         Text(pet.additionalInformation ?? "No info"),
         const SizedBox(height: 20),
         const Text("Feeding History:", style: TextStyle(fontSize: 18)),
      ],
    );
  }
}