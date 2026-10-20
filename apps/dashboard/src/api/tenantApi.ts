import type { tenantCreateRequest } from './types';

export async function createTenant(accestoken: string, request: tenantCreateRequest){
    const response = await fetch(`${import.meta.env.VITE_TENANT_API_URL}/tenant/details`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${accestoken}`
        },
        body: JSON.stringify(request)
    });

    if (!response.ok) {
        throw new Error(`Failed to fetch tenant details: ${response.statusText}`);
    }

    return response.json();

}
