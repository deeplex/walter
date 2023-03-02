<script lang="ts">
	import type { ErhaltungsaufwendungEntry } from '$types';
	import {
		WalterGrid,
		WalterErhaltungsaufwendung,
		WalterHeaderNew
	} from '$components';

	const url = `/api/erhaltungsaufwendungen`;
	const entry: Partial<ErhaltungsaufwendungEntry> = {};

	const title = 'Neue Erhaltungsaufwendung';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterErhaltungsaufwendung {entry} />
</WalterGrid>
