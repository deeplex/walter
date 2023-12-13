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
        console.log(response);
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
