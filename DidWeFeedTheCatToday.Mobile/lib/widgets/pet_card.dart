import 'package:flutter/material.dart';
import 'package:pet_api_mobile/models/pet_dto.dart';
import 'package:pet_api_mobile/screens/pet_details_screen.dart';

class PetCard extends StatelessWidget{
  final PetDto pet;
  final VoidCallback onFeed;

  const PetCard({super.key, required this.pet, required this.onFeed});

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 4,
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: InkWell(
        onTap: (){
          Navigator.push(context, MaterialPageRoute(builder: (context) => PetDetailsScreen(petId: pet.id, petName: pet.name)));
        },
        child: ListTile(
          leading: const CircleAvatar(
            child: Icon(Icons.pets),
          ),
          title: Text(pet.name, style: const TextStyle(fontWeight: FontWeight.bold)),
          subtitle: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Last fed: ${pet.lastFed?.toLocal().toString().split('.')[0] ?? 'Never'}'),
              if (pet.additionalInformation != null)
                Text(
                  pet.additionalInformation!,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontSize: 12),
                )
            ],
          ),
          trailing: IconButton(
            onPressed: onFeed, 
            icon: const Icon(Icons.restaurant, color: Colors.green,)),
          ),
      ),
      );
  }
}