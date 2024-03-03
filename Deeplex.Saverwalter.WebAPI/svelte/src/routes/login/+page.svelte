<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import { WalterHeader } from '$walter/components';

    import {
        Button,
        Content,
        FluidForm,
        PasswordInput,
        TextInput
    } from 'carbon-components-svelte';
    import { Login } from 'carbon-icons-svelte';
    import { WalterToastContent } from '$walter/lib';
    import { authState, walter_sign_in } from '$walter/services/auth';
    import type { PageData } from './$types';
    import { page } from '$app/stores';
    import { walter_goto } from '$walter/services/utils';
    import { get } from 'svelte/store';

    export let data: PageData;

    const login = {
        username: '',
        password: ''
    };

    let invalid = false;

    const LoginToast = new WalterToastContent(
        'Anmeldung erfolgreich',
        'Anmeldung fehlgeschlagen',
        () => `Willkommen ${get(authState!)?.name}`,
        () => 'Anmeldung fehlgeschlagen.'
    );

    async function submit() {
        const response = await walter_sign_in(
            data.fetch,
            login.username,
            login.password,
            LoginToast
        );

        if (response == null) {
            invalid = true;
        } else {
            if (document.referrer.includes($page.url.host)) {
                history.back(); // TODO should be walter_history_back
            } else {
                walter_goto('/');
            }
        }
    }

    function handleEnterKey(event: KeyboardEvent) {
        if (event.key === 'Enter') {
            submit();
        }
    }
</script>

<Content>
    <WalterHeader title="Anmeldeseite" />
    <FluidForm style="text-align: center; margin-top: 40vh;">
        <div style="max-width: 40em; margin: auto">
            <TextInput
                bind:value={login.username}
                labelText="Nutzername"
                bind:invalid
                invalidText="Nutzername oder Passwort falsch"
                placeholder="Nutzername eintragen..."
                required
            />
            <PasswordInput
                bind:value={login.password}
                bind:invalid
                invalidText="Nutzername oder Passwort falsch"
                required
                type="password"
                labelText="Passwort"
                placeholder="Passwort eintragen..."
                on:keydown={handleEnterKey}
            />
            <Button on:click={submit} icon={Login}>Anmelden</Button>
        </div>
    </FluidForm>
</Content>
