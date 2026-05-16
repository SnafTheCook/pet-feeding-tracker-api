import 'package:flutter/material.dart';
import 'package:pet_api_mobile/models/result.dart';
import 'package:pet_api_mobile/providers/pet_provider.dart';
import 'package:provider/provider.dart';

class AddPetScreen extends StatefulWidget {
  const AddPetScreen({super.key});

  @override
  State<AddPetScreen> createState() => _AddPetScreenState();
}

class _AddPetScreenState extends State<AddPetScreen> {
  final _formKey = GlobalKey<FormState>();

  String _name = '';
  int? _age;
  String? _info;

  bool _isSaving = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Add new pet')),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: ListView(
            children: [
              TextFormField(
                decoration: const InputDecoration(labelText: 'Pet name'),
                validator: (value) {
                  if (value == null || value.isEmpty) return 'Please enter a name';
                  return null;
                },
                onSaved: (value) => _name = value ?? '',
              ),
              const SizedBox(height: 16),
              TextFormField(
                decoration: const InputDecoration(labelText: 'Age (optional)'),
                keyboardType: TextInputType.number,
                onSaved: (value) => _age = int.tryParse(value ?? ''),
              ),
              const SizedBox(height: 16),
              TextFormField(
                decoration: const InputDecoration(labelText: 'Additional Info'),
                maxLines: 3,
                onSaved: (value) => _info = value,
              ),
              const SizedBox(height: 32),
              ElevatedButton(
                onPressed: _isSaving
                    ? null
                    : _submitForm, // Disable button while saving!
                child: _isSaving
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Text('Save Pet'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _submitForm() async {
    if (_formKey.currentState!.validate()) {
      setState(() => _isSaving = true);

      _formKey.currentState!.save();

      final result = await context.read<PetProvider>().addPet(_name, _age, _info);
      if (!mounted) return;

      switch (result) {
        case Success():
          setState(() => _isSaving = false);
          Navigator.pop(context);
          break;
        case Failure(message: var msg):
          setState(() => _isSaving = false);
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(msg), backgroundColor: Colors.red),
          );
          break;
      }
    }
  }
}
